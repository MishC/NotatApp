using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderRepository
{
    Task<List<Folder>> GetAllFoldersAsync(string? userId);
    Task<Folder?> GetFolderByIdAsync(int id, string? userId);
    Task<Folder?> GetFolderByNameAsync(string name, string? userId);

    Task AddFolderAsync(Folder folder, string userId);
    Task UpdateFolderAsync(Folder folder, string userId);
    Task DeleteFolderAsync(Folder folder, string userId);
    Task DeleteFolderByIdAsync(int id, string userId);
}
