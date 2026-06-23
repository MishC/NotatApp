using NotatApp.Models;

namespace NotatApp.Services.DiaryServices
{
    public interface IRecommendations
    {
        Task<List<RecommendedSong>> GetAnswerOnPrompt(
            string userId,
            DiaryEntry diaryEntry,
            string? style,
            string? country);
    }
}
