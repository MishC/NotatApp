using System.Text.Json;
using System.ClientModel;
using OpenAI;
using OpenAI.Responses;
using NotatApp.Models;

#pragma warning disable OPENAI001

namespace NotatApp.Services.DiaryServices
{
    public class Recommendations : IRecommendations
    {
        private readonly IConfiguration _config;

        public Recommendations(IConfiguration config)
        {
            _config = config;
        }

        public async Task<List<RecommendedSong>> GetAnswerOnPrompt(
            string userId,
            DiaryEntry diaryEntry,
            string? style)
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

            options.InputItems.Add(ResponseItem.CreateUserMessageItem(BuildRecommendationPrompt(content, style)));

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

            return ParseRecommendedSongs(output, model, diaryEntry.Id);
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

        private static string BuildRecommendationPrompt(string content, string? style)
        {
            var preferredType = string.IsNullOrWhiteSpace(style)
                ? "No specific style. Choose what fits the diary best."
                : style;

            var additionalType = "Choose ";
            if (style == "International")
            {
                additionalType = "Choose international hits from radio.";
            }
            else if (style == "Local")
            {
                additionalType = "Choose local hits from radio. Hitds for that land, cultural context is important.";
            }
            else
            {
                additionalType = "Choose classical music or modern electronic music (just melody, no words).";
            }

            return $$"""
                Recommend song based on this diary text or mood:

                {{content}}

                Preferred type of songs:
                {{preferredType}}
                {{additionalType}}
 
                Return only valid JSON. Do not wrap it in markdown.
                JSON shape:
                [
                  {
                    "title": "song title",
                    "artist": "artist name",
                    "link": "optional youtube public URL or null"
                  }
                ]
                Choose a song based on:

                The overall atmosphere of the story.
                The emotions and mood.
                Cultural associations.
                The theme of the story.
                Similarity of lyrics, but only as an additional criterion.
                
                Return exactly just 1 song.
                """;
        }

        private static List<RecommendedSong> ParseRecommendedSongs(string output, string model, int diaryEntryId)
        {
            if (string.IsNullOrWhiteSpace(output))
                return [];

            var songs = JsonSerializer.Deserialize<List<RecommendedSongResponse>>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

            return [.. songs
                .Where(song => !string.IsNullOrWhiteSpace(song.Title))
                .Select(song => new RecommendedSong
                {
                    Title = song.Title.Trim(),
                    Artist = song.Artist?.Trim() ?? string.Empty,
                    Link = string.IsNullOrWhiteSpace(song.Link) ? null : song.Link.Trim(),
                    Model = model,
                    DiaryEntryId = diaryEntryId
                })];
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
