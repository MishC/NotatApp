using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
    {
        return await _folderRepository.GetAllFoldersAsync();
    }

    public async Task<Folder> GetFolderByIdAsync(int id)
    {
        return await _folderRepository.GetFolderByIdAsync(id);
    }

    public async Task<Folder> AddFolderAsync(Folder folder)
    {
        return await _folderRepository.AddFolderAsync(folder);
    }

    public async Task<bool> UpdateFolderAsync(Folder folder)
    {
        return await _folderRepository.UpdateFolderAsync(folder);
    }

    public async Task<bool> DeleteFolderAsync(int id)
    {
        return await _folderRepository.DeleteFolderAsync(id);
    }
}
