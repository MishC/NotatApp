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

        public Recommendations(
            IConfiguration config,
            IRecommendedSongRepository recommendedSongRepository)
        {
            _config = config;
            _recommendedSongRepository = recommendedSongRepository;
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

            var model = _config["OpenAI:Model"] ?? "gpt-5-mini";
            var normalizedStyle = NormalizeText(style);
            var normalizedCountry = NormalizeText(country);
            var prompt = BuildRecommendationPrompt(
                BuildSongRecommendationPrompt(normalizedStyle, normalizedCountry),
                content);
            var output = await GetOpenAiOutputAsync(prompt, userId, 800);

            var song = ParseRecommendedSongs(output, model, diaryEntry.Id, normalizedStyle, normalizedCountry)
                .FirstOrDefault();

            if (song is null)
                return [];

            song.Link = GetSafeSongLink(BuildSongSearchLink(song));

            var savedSong = await _recommendedSongRepository.SaveForDiaryEntryAsync(diaryEntry.Id, song);
            return [savedSong];
        }

        public async Task<string> GetFrame(string description)
        {
            var content = NormalizeText(description);
            if (content is null)
                throw new ArgumentException("Description is required.", nameof(description));

            var prompt = BuildRecommendationPrompt(
                """
                Write a CSS string based on the description.
                The CSS will be applied inline to a diary frame container.
                Return only CSS declarations. Do not return a selector, braces, markdown, JSON, comments, or explanation.
                Use only frame color or gradient styling.
                Allowed properties: background, background-color, border, border-color, box-shadow.
                Do not use url(), image URLs, scripts, animation, position, transform, width, height, padding, margin, or content.
                Keep it compact and visually polished.
                """,
                content);

            var output = await GetOpenAiOutputAsync(prompt, null, 250);
            return SanitizeFrameCss(output);
        }

        private async Task<string> GetOpenAiOutputAsync(
            string prompt,
            string? userId,
            int maxOutputTokenCount)
        {
            var apiKey = _config["OpenAI:ApiKey"]
                ?? throw new InvalidOperationException("OpenAI API key is not configured.");

            var model = _config["OpenAI:Model"] ?? "gpt-5-mini";
            var client = new ResponsesClient(apiKey);

            var options = new CreateResponseOptions
            {
                Model = model,
                MaxOutputTokenCount = maxOutputTokenCount,
                StoredOutputEnabled = false
            };

            if (!string.IsNullOrWhiteSpace(userId))
                options.EndUserId = userId;

            options.InputItems.Add(ResponseItem.CreateUserMessageItem(prompt));

            try
            {
                ResponseResult response = await client.CreateResponseAsync(options);
                return response.GetOutputText();
            }
            catch (ClientResultException ex)
            {
                throw new InvalidOperationException($"OpenAI request failed: {ex.Message}", ex);
            }
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

        private static string BuildRecommendationPrompt(string prompt, string content)
        {
            return $$"""
                {{prompt}}

                Content/description:
                {{content}}
                """;
        }

        private static string BuildSongRecommendationPrompt(string? style, string? country)
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
                    "link": null
                  }
                ]
                Always return link as null. The backend creates the YouTube search link.
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

        private static string SanitizeFrameCss(string output)
        {
            var css = NormalizeText(output)
                ?.Replace("```css", "", StringComparison.OrdinalIgnoreCase)
                .Replace("```", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (string.IsNullOrWhiteSpace(css))
                throw new InvalidOperationException("OpenAI returned an empty frame CSS response.");

            var openBraceIndex = css.IndexOf('{');
            var closeBraceIndex = css.LastIndexOf('}');
            if (openBraceIndex >= 0 && closeBraceIndex > openBraceIndex)
                css = css[(openBraceIndex + 1)..closeBraceIndex];

            var declarations = css
                .ReplaceLineEndings(" ")
                .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(SanitizeFrameDeclaration)
                .Where(declaration => declaration is not null)
                .ToList();

            if (declarations.Count == 0)
                throw new InvalidOperationException("OpenAI returned frame CSS without allowed declarations.");

            return string.Join("; ", declarations) + ";";
        }

        private static string? SanitizeFrameDeclaration(string declaration)
        {
            var colonIndex = declaration.IndexOf(':');
            if (colonIndex <= 0 || colonIndex >= declaration.Length - 1)
                return null;

            var property = declaration[..colonIndex].Trim().ToLowerInvariant();
            var value = declaration[(colonIndex + 1)..].Trim();

            if (!IsAllowedFrameCssProperty(property) || ContainsBlockedCssValue(value))
                return null;

            return $"{property}: {value}";
        }

        private static bool IsAllowedFrameCssProperty(string property)
        {
            return property is "background"
                or "background-color"
                or "border"
                or "border-color"
                or "box-shadow";
        }

        private static bool ContainsBlockedCssValue(string value)
        {
            var lowerValue = value.ToLowerInvariant();
            return lowerValue.Contains("url(")
                || lowerValue.Contains("javascript:")
                || lowerValue.Contains("expression(")
                || lowerValue.Contains("@import")
                || lowerValue.Contains("<")
                || lowerValue.Contains(">")
                || lowerValue.Contains("{")
                || lowerValue.Contains("}");
        }

        private static string? BuildSongSearchLink(RecommendedSong song)
        {
            var label = BuildSongLabel(song);
            if (string.IsNullOrWhiteSpace(label) || label == "Recommended song")
                return null;

            return $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(label)}";
        }

        private static string? GetSafeSongLink(string? link)
        {
            var value = NormalizeText(link);
            if (value is null)
                return null;

            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            var host = uri.Host.ToLowerInvariant();
            if (host.StartsWith("m.", StringComparison.Ordinal))
                host = host[2..];
            if (host.StartsWith("www.", StringComparison.Ordinal))
                host = host[4..];

            var isYouTubeHost = host == "youtube.com"
                || host.EndsWith(".youtube.com", StringComparison.Ordinal)
                || host == "youtu.be";

            if (!isYouTubeHost)
                return uri.ToString();

            return uri.AbsolutePath == "/results" ? uri.ToString() : null;
        }

        private static string BuildSongLabel(RecommendedSong song)
        {
            var artist = song.Artist.Trim();
            var title = song.Title.Trim();

            if (!string.IsNullOrWhiteSpace(artist) && !string.IsNullOrWhiteSpace(title))
                return $"{artist} - {title}";
            return string.IsNullOrWhiteSpace(artist) ? title : artist;
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
