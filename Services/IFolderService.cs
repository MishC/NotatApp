using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderService
{
    Task<List<Folder>> GetAllFoldersAsync();
    Task<Folder> GetFolderByIdAsync(int id);
    Task<Folder?> GetFolderByNameAsync(string name);

    Task<Folder> AddFolderAsync(Folder folder, string userId);
    Task<bool> UpdateFolderAsync(Folder folder, string userId);
    //Task<bool> DeleteFolderAsync(int id, string userId);
    Task<bool> DeleteFolderByIdAsync(int id, string userId);
    Task<string> GetFolderNameByIdAsync(int id);
}
