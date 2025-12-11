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

        public async Task<List<Note>> GetAllNotesAsync(string userId) =>
            await _context.Notes
                .Where(n => n.UserId == userId)
                .Include(n => n.Folder)
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();


      public async Task<List<Note>> GetDoneNotesAsync(string userId) =>
      await _context.Notes
        .Include(n => n.Folder)
        .Where(n => n.UserId == userId 
                    && n.Folder != null 
                    && n.Folder.Name == "Done")
        .OrderBy(n => n.OrderIndex)
        .ToListAsync();


        public async Task<Note?> GetNoteByIdAsync(int id, string userId) =>
            await _context.Notes
                .Include(n => n.Folder)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        public async Task AddNoteAsync(Note note) //create Note
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int id, string userId)
        {
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SwapOrderAsync(int sourceId, int targetId, string userId)
        {
            var source = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == sourceId && n.UserId == userId);
            var target = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == targetId && n.UserId == userId);

            if (source == null || target == null)
                return;

            var tmp = source.OrderIndex;
            source.OrderIndex = target.OrderIndex;
            target.OrderIndex = tmp;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Note>> GetPendingNotesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            return await _context.Notes
                .Where(n => n.UserId == userId && !n.IsArchived)
                .OrderBy(n => n.OrderIndex)
                .ToListAsync();
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
