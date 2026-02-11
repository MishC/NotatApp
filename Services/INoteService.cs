using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

namespace NotatApp.Services
{
    public interface INoteService
    {
        Task<List<Note>> GetAllNotesAsync(string userId);
        Task<List<Note>> GetPendingNotesAsync(string userId);
        Task<List<Note>> GetDoneNotesAsync(string userId);

        Task<List<Note>> GetOverdueNotesAsync(string userId);

        Task<Note?> GetNoteByIdAsync(int id, string userId);
        Task<List<Note?>> GetNotesByFolderIdAsync(int folderId, string userId);

        Task<Note> CreateNoteAsync( CreateNoteDto dto, string userId);
        Task<bool> UpdateNoteAsync(int id, UpdateNoteDto dto, string userId);
        Task<bool> UpdateNoteFolderAsync(int id, int folderId, string userId);
        Task<bool> DeleteNoteAsync(int id, string userId);

        Task<bool> IsNoteOverdueAsync(int id, string userId);

        Task SwapOrderAsync(int sourceId, int targetId, string userId);
    }
}
