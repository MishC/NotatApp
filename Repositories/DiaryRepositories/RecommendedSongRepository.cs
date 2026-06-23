using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Models;

namespace NotatApp.Repositories.DiaryRepositories
{
    public class RecommendedSongRepository : IRecommendedSongRepository
    {
        private readonly ApplicationDbContext _context;

        public RecommendedSongRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RecommendedSong> SaveForDiaryEntryAsync(int diaryEntryId, RecommendedSong song)
        {
            var style = Normalize(song.Style);
            var country = Normalize(song.Country);

            var existing = await _context.RecommendedSongs
                .FirstOrDefaultAsync(recommendation =>
                    recommendation.DiaryEntryId == diaryEntryId
                    && recommendation.Style == style
                    && recommendation.Country == country);

            song.Style = style;
            song.Country = country;
            if (existing is null)
            {
                song.DiaryEntryId = diaryEntryId;
                await _context.RecommendedSongs.AddAsync(song);
                await _context.SaveChangesAsync();
                return song;
            }

            existing.Title = song.Title;
            existing.Artist = song.Artist;
            existing.Link = song.Link;
            existing.Model = song.Model;
            existing.Style = song.Style;
            existing.Country = song.Country;
            existing.Like = song.Like;
            existing.CreatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        private static string? Normalize(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
    }
}
