using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Models;


namespace NotatApp.Repositories.DiaryRepositories
{
    public class DiaryRepository(ApplicationDbContext context) : IDiaryRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<List<DiaryEntry>> GetByDateAsync(string userId, DateOnly date)
        {
            return await _context.DiaryEntries
                .Where(e => e.UserId == userId && e.Date == date)
                .OrderByDescending(e => e.Id)
                .ToListAsync();
        }

        public async Task<DiaryEntry?> GetByIdAsync(int id, string userId)
        {
            return await _context.DiaryEntries
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task AddAsync(DiaryEntry entry)
        {
            await _context.DiaryEntries.AddAsync(entry);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}