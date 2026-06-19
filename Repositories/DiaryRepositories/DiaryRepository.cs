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
                .Include(e => e.Pages)
                .Where(e => e.UserId == userId && e.Date == date)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync();
        }

        public async Task<DiaryEntry?> GetByIdAsync(int id, string userId)
        {
            return await _context.DiaryEntries
                .Include(e => e.Pages)
                .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);
        }

        public async Task<DiaryPage?> GetPageByIdAsync(int pageId, string userId)
        {
            return await _context.DiaryPages
                .Include(p => p.DiaryEntry)
                .FirstOrDefaultAsync(p => p.Id == pageId && p.DiaryEntry.UserId == userId);
        }

        public async Task AddAsync(DiaryEntry entry)
        {
            await _context.DiaryEntries.AddAsync(entry);
        }

        public async Task AddPageAsync(DiaryPage page)
        {
            await _context.DiaryPages.AddAsync(page);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public Task DeleteAsync(DiaryEntry entry)
        {
            _context.DiaryEntries.Remove(entry);
            return Task.CompletedTask;
        }

        public Task DeletePageAsync(DiaryPage page)
        {
            _context.DiaryPages.Remove(page);
            return Task.CompletedTask;
        }

        public Task DeleteByDateAsync(List<DiaryEntry> entries)
        {
            _context.DiaryEntries.RemoveRange(entries);
            return Task.CompletedTask;
        }
    }
}
