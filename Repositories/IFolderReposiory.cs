using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderRepository
{
    Task<List<Folder>> GetAllFoldersAsync();
    Task<Folder?> GetFolderByIdAsync(int id);

    Task AddFolderAsync(Folder folder, string userId);
    Task UpdateFolderAsync(Folder folder, string userId);
    Task DeleteFolderAsync(Folder folder, string userId);
    Task DeleteFolderByIdAsync(int id, string userId);
}
