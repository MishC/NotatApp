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

        Task AddAsync(Note note);
        Task UpdateAsync(Note note);
        Task DeleteAsync(Note note);

        Task<int> GetNextOrderIndexAsync(string userId);
    }
}
