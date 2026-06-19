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
                .OrderBy(e => e.Date)
                .ToListAsync();
        }

        public async Task<DiaryEntry?> GetByIdAsync(int id, string userId)
        {
            return await _context.DiaryEntries
                .Include(e => e.Pages)
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



        public async Task<bool> DeleteAsync(DiaryEntry entry)
        {
    
            _context.DiaryEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByDateAsync(List<DiaryEntry> entries)        
        {
        
            _context.DiaryEntries.RemoveRange(entries);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
