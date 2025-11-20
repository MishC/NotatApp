
using NotatApp.Models;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository)); //DI
    }

    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
    {
        return await _folderRepository.GetAllFoldersAsync();
    }

    public async Task<Folder> GetFolderByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));
        }

        var folder = await _folderRepository.GetFolderByIdAsync(id);
        if (folder == null)
        {
            throw new KeyNotFoundException($"Folder with ID {id} not found.");
        }

        return folder;
    }

    public async Task<Folder> AddFolderAsync(Folder folder)
    {
        if (folder == null)
        {
            throw new ArgumentNullException(nameof(folder), "Folder cannot be null.");
        }

        if (string.IsNullOrWhiteSpace(folder.Name) || folder.Name.Length < 3 || folder.Name.Length > 50)
        {
            throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folder.Name));
        }

        return await _folderRepository.AddFolderAsync(folder);
    }

    public async Task<bool> UpdateFolderAsync(Folder folder)
    {
        if (folder == null)
        {
            throw new ArgumentNullException(nameof(folder), "Folder cannot be null.");
        }

        var existingFolder = await _folderRepository.GetFolderByIdAsync(folder.Id);
        if (existingFolder == null)
        {
            throw new KeyNotFoundException($"Folder with ID {folder.Id} not found.");
        }

        if (string.IsNullOrWhiteSpace(folder.Name) || folder.Name.Length < 3 || folder.Name.Length > 50)
        {
            throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folder.Name));
        }

        return await _folderRepository.UpdateFolderAsync(folder);
    }

    public async Task<bool> DeleteFolderAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));
        }

        var folder = await _folderRepository.GetFolderByIdAsync(id);
        if (folder == null)
        {
            throw new KeyNotFoundException($"Folder with ID {id} not found.");
        }

        return await _folderRepository.DeleteFolderAsync(id);
    }
}
