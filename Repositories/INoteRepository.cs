using NotatApp.Models;

namespace NotatApp.Repositories
{
    public interface INoteRepository
    {
        Task<List<Note>> GetAllNotesAsync(string userId);
        Task<List<Note>> GetPendingNotesAsync(string userId);
        Task<List<Note>> GetDoneNotesAsync(string userId);
        Task<Note?> GetNoteByIdAsync(int id, string userId);
        Task<int> GetNextOrderIndexAsync(string userId);


        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(int id, string userId);

        Task SwapOrderAsync(int sourceId, int targetId, string userId);
    }
}
