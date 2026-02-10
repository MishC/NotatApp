
using Microsoft.EntityFrameworkCore;
using NotatApp.Data;
using NotatApp.Models;

namespace NotatApp.Repositories
{
    public class NoteRepository : INoteRepository
    {
        private readonly ApplicationDbContext _context;

        public NoteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetUserNotesAsync(string userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId)
                .Include(n => n.Folder)
                .ToListAsync();
        }

        public async Task<Note?> GetNoteByIdAsync(int id, string userId)
        {
            return await _context.Notes
                .Include(n => n.Folder)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task<List<Note>> GetNotesByFolderIdAsync(int folderId, string userId)
        {
            return await _context.Notes
                .Where(n => n.FolderId == folderId && n.UserId == userId)
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();
        }

        public async Task AddAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Note note)
        {
            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetNextOrderIndexAsync(string userId)
        {
            int? maxIndex = await _context.Notes
                .Where(n => n.UserId == userId)
                .MaxAsync(n => (int?)n.OrderIndex);

            return (maxIndex ?? -1) + 1;
        }
    }
}
