using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

namespace NotatApp.Repositories
{
    public interface INoteRepository
    {
        Task<List<Note>> GetUserNotesAsync(string userId);
        Task<Note?> GetNoteByIdAsync(int id, string userId);
        Task<List<Note?>> GetNotesByFolderIdAsync(int folderId, string userId);

        Task AddNoteAsync(Note note);
        Task UpdateNoteAsync(Note note);
        Task DeleteNoteAsync(Note note);

        Task<int> GetNextOrderIndexAsync(string userId);
    }
}
