using NotatApp.Models;

namespace NotatApp.Services
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync();
        Task<IEnumerable<Note>> GetPendingNotesAsync();
        Task<IEnumerable<Note>> GetDoneNotesAsync();
        Task<Note?> GetNoteByIdAsync(int id);
        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id);
    }
}
