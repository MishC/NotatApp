using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderService
{
    Task<List<Folder>> GetAllFoldersAsync();
    Task<Folder> GetFolderByIdAsync(int id);
    Task<Folder?> GetFolderByNameAsync(string name);

    Task<Folder> AddFolderAsync(Folder folder);
    Task<bool> UpdateFolderAsync(Folder folder);
    Task<bool> DeleteFolderAsync(int id);
    Task<string> GetFolderNameByIdAsync(int id);
}
