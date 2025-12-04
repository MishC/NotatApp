using NotatApp.Models;

namespace NotatApp.Services
{
    public interface INoteService
    {
        Task<IReadOnlyList<Note>> GetAllNotesAsync(string userId);
        Task<IReadOnlyList<Note>> GetPendingNotesAsync(string userId);
        Task<IReadOnlyList<Note>> GetDoneNotesAsync(string userId);
        Task<Note?> GetNoteByIdAsync(int id, string userId);

        Task<Note> CreateNoteAsync(CreateNoteDto dto, string userId);
        Task<bool> UpdateNoteAsync(int id, UpdateNoteDto dto, string userId);
        Task<bool> UpdateNoteFolderAsync(int id, int folderId, string userId);
        Task<bool> DeleteNoteAsync(int id, string userId);
        Task SwapOrderAsync(int sourceId, int targetId, string userId);
    }
}
