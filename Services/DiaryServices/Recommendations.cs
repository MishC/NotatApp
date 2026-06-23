using System.Net;
using System.Text.Json;
using System.ClientModel;
using OpenAI;
using OpenAI.Responses;
using NotatApp.Models;
using NotatApp.Repositories.DiaryRepositories;

#pragma warning disable OPENAI001

namespace NotatApp.Services.DiaryServices
{
    public class Recommendations : IRecommendations
    {
        private readonly IConfiguration _config;
        private readonly IRecommendedSongRepository _recommendedSongRepository;
        private readonly IHttpClientFactory _httpClientFactory;

        public Recommendations(
            IConfiguration config,
            IRecommendedSongRepository recommendedSongRepository,
            IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _recommendedSongRepository = recommendedSongRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<RecommendedSong>> GetAnswerOnPrompt(
            string userId,
            DiaryEntry diaryEntry,
            string? style,
            string? country)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            if (diaryEntry is null)
                throw new ArgumentNullException(nameof(diaryEntry));

            var content = BuildDiaryContent(diaryEntry);
            if (string.IsNullOrWhiteSpace(content))
                throw new ArgumentException("Diary entry does not contain any page content.", nameof(diaryEntry));

            var apiKey = _config["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key is not configured.");

            var model = _config["OpenAI:Model"] ?? "gpt-5-mini";
            var client = new ResponsesClient(apiKey);

            var options = new CreateResponseOptions
            {
                Model = model,
                EndUserId = userId,
                MaxOutputTokenCount = 800,
                StoredOutputEnabled = false
            };

            var normalizedStyle = NormalizeText(style);
            var normalizedCountry = NormalizeText(country);

            options.InputItems.Add(ResponseItem.CreateUserMessageItem(
                BuildRecommendationPrompt(content, normalizedStyle, normalizedCountry)));

            ResponseResult response;
            try
            {
                response = await client.CreateResponseAsync(options);
            }
            catch (ClientResultException ex)
            {
                throw new InvalidOperationException($"OpenAI request failed: {ex.Message}", ex);
            }

            var output = response.GetOutputText();

            var song = ParseRecommendedSongs(output, model, diaryEntry.Id, normalizedStyle, normalizedCountry)
                .FirstOrDefault();

            if (song is null)
                return [];

            song.Link = await GetAllowedLinkAsync(song.Link);

            var savedSong = await _recommendedSongRepository.SaveForDiaryEntryAsync(diaryEntry.Id, song);
            return [savedSong];
        }

        private static string BuildDiaryContent(DiaryEntry diaryEntry)
        {
            return string.Join(
                "\n\n",
                diaryEntry.Pages
                    .OrderBy(page => page.PageNumber)
                    .Select(page => page.Content?.Trim())
                    .Where(content => !string.IsNullOrWhiteSpace(content)));
        }

        private static string BuildRecommendationPrompt(string content, string? style, string? country)
        {
            var preferredType = string.IsNullOrWhiteSpace(style)
                ? "No specific style. Choose what fits the diary best."
                : style;

            var additionalType = "Choose songs that fit the diary mood, emotion, and story.";
            if (IsStyle(style, "International"))
            {
                additionalType = "Choose well-known international songs that are likely available on public music platforms.";
            }
            else if (IsStyle(style, "Local"))
            {
                additionalType = string.IsNullOrWhiteSpace(country)
                    ? "Choose local hits from the user's country or region if it is clear from the diary text."
                    : $"Choose local songs, artists, or radio hits from {country}. Prefer artists culturally connected to {country}.";
            }
            else if (!string.IsNullOrWhiteSpace(country))
            {
                additionalType = $"The user provided country: {country}. Prefer songs or artists local to {country}, unless the diary mood clearly needs an international song.";
            }
            else
            {
                additionalType = "Choose a song style that naturally matches the diary. Classical, modern electronic, pop, indie, or acoustic are all allowed.";
            }

            return $$"""
                Recommend song based on this diary text or mood:

                {{content}}

                Preferred type of songs:
                {{preferredType}}
                Country:
                {{(string.IsNullOrWhiteSpace(country) ? "No country provided." : country)}}
                {{additionalType}}
 
                Return only valid JSON. Do not wrap it in markdown.
                JSON shape:
                [
                  {
                    "title": "song title",
                    "artist": "artist name",
                    "link": "optional public YouTube watch URL or null"
                  }
                ]
                Link must be a normal public YouTube page URL, such as https://www.youtube.com/watch?v=VIDEO_ID.
                Never return googlevideo.com, videoplayback, embed, player, or temporary media stream URLs.
                Choose a song based on:

                The overall atmosphere of the story.
                The emotions and mood.
                Cultural associations.
                The theme of the story.
                Similarity of lyrics, but only as an additional criterion.

                Return max 1 song
                """;
        }

        private static List<RecommendedSong> ParseRecommendedSongs(
            string output,
            string model,
            int diaryEntryId,
            string? style,
            string? country)
        {
            if (string.IsNullOrWhiteSpace(output))
                return [];

            List<RecommendedSongResponse> songs;
            try
            {
                songs = JsonSerializer.Deserialize<List<RecommendedSongResponse>>(
                    output,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException("OpenAI returned an invalid song recommendation format.", ex);
            }

            return [.. songs
                .Where(song => !string.IsNullOrWhiteSpace(song.Title))
                .Take(1)
                .Select(song => new RecommendedSong
                {
                    Title = song.Title.Trim(),
                    Artist = song.Artist?.Trim() ?? string.Empty,
                    Link = string.IsNullOrWhiteSpace(song.Link) ? null : song.Link.Trim(),
                    Model = model,
                    Style = style,
                    Country = country,
                    DiaryEntryId = diaryEntryId
                })];
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static bool IsStyle(string? style, string expected)
        {
            return string.Equals(style?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<string?> GetAllowedLinkAsync(string? link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return null;

            var trimmedLink = link.Trim();
            if (!Uri.TryCreate(trimmedLink, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            if (IsBlockedYouTubeMediaUrl(uri))
                return null;

            if (!IsAllowedYouTubePageUrl(uri))
                return trimmedLink;

            var client = _httpClientFactory.CreateClient();

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, uri);
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                if (IsRejectedLinkStatus(response.StatusCode))
                    return null;

                if (response.StatusCode != HttpStatusCode.MethodNotAllowed)
                    return trimmedLink;

                using var getRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                using var getResponse = await client.SendAsync(getRequest, HttpCompletionOption.ResponseHeadersRead);

                return IsRejectedLinkStatus(getResponse.StatusCode) ? null : trimmedLink;
            }
            catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (TaskCanceledException)
            {
                return trimmedLink;
            }
            catch (HttpRequestException)
            {
                return trimmedLink;
            }
        }

        private static bool IsBlockedYouTubeMediaUrl(Uri uri)
        {
            return uri.Host.EndsWith("googlevideo.com", StringComparison.OrdinalIgnoreCase)
                || uri.AbsolutePath.Contains("videoplayback", StringComparison.OrdinalIgnoreCase)
                || uri.AbsolutePath.Contains("/embed/", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsAllowedYouTubePageUrl(Uri uri)
        {
            return uri.Host.EndsWith("youtube.com", StringComparison.OrdinalIgnoreCase)
                || uri.Host.EndsWith("youtu.be", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsRejectedLinkStatus(HttpStatusCode statusCode)
        {
            return statusCode is HttpStatusCode.Forbidden or HttpStatusCode.NotFound;
        }

        private sealed class RecommendedSongResponse
        {
            public string Title { get; set; } = string.Empty;
            public string? Artist { get; set; }
            public string? Link { get; set; }
        }
    }
}

#pragma warning restore OPENAI001
