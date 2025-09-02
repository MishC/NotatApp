using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        public async Task<List<Note>> GetAllNotesAsync()
        {
            var notes = _context.Notes
        .Where(n => n.FolderId == folderId)
        .OrderBy(n => n.OrderIndex)
        .ToList();

        return notes;

        }


        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            return await _context.Notes
                .Include(n => n.Folder) // Ensure Folder is loaded
                .FirstOrDefaultAsync(n => n.Id == id);
        }
        
        public async Task AddNoteAsync(Note note)
        {
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(Note note)
        {
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }
    }
}
