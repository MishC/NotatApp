using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderService
{
    Task<List<Folder>> GetAllFoldersAsync(string? userId);
    Task<Folder> GetFolderByIdAsync(int id, string? userId);
    Task<Folder?> GetFolderByNameAsync(string name, string? userId);

    Task<Folder> AddFolderAsync(CreateFolderDto folderDto, string userId);
    Task<bool> UpdateFolderAsync(int id, UpdateFolderDto folderDto, string userId);
    //Task<bool> DeleteFolderAsync(int id, string userId);
    Task<bool> DeleteFolderByIdAsync(int id, string userId);
    Task<string> GetFolderNameByIdAsync(int id, string? userId);
}
