using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderRepository
{
    Task<IEnumerable<Folder>> GetAllFoldersAsync();
    Task<Folder> GetFolderByIdAsync(int id);
    Task<Folder> AddFolderAsync(Folder folder);
    Task<bool> UpdateFolderAsync(Folder folder);
    Task<bool> DeleteFolderAsync(int id);
}
