using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderRepository
{
    Task<List<Folder>> GetAllFoldersAsync();
    Task<Folder?> GetFolderByIdAsync(int id);

    Task AddFolderAsync(Folder folder);
    Task UpdateFolderAsync(Folder folder);
    Task DeleteFolderAsync(Folder folder);
}
