
using NotatApp.Models;

public class FolderService : IFolderService
{
    private readonly IFolderRepository _folderRepository;

    public FolderService(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository ?? throw new ArgumentNullException(nameof(folderRepository));
    }

    public async Task<List<Folder>> GetAllFoldersAsync()
    {
        var folders = await _folderRepository.GetAllAsync();
        // Optional: sort by name
        return folders
            .OrderBy(f => f.Name)
            .ToList();
    }

    public async Task<Folder> GetFolderByIdAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));

        var folder = await _folderRepository.GetByIdAsync(id);
        if (folder == null)
            throw new KeyNotFoundException($"Folder with ID {id} not found.");

        return folder;
    }

    public async Task<Folder?> GetFolderByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim().ToLowerInvariant();
        var folders = await _folderRepository.GetAllAsync();

        return folders.FirstOrDefault(f =>
            !string.IsNullOrEmpty(f.Name) &&
            f.Name.Trim().ToLowerInvariant() == normalized);
    }

    public async Task<Folder> AddFolderAsync(Folder folder)
    {
        if (folder == null)
            throw new ArgumentNullException(nameof(folder), "Folder cannot be null.");

        if (string.IsNullOrWhiteSpace(folder.Name) || folder.Name.Length < 3 || folder.Name.Length > 50)
            throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folder.Name));

        await _folderRepository.AddAsync(folder);
        return folder;
    }

    public async Task<bool> UpdateFolderAsync(Folder folder)
    {
        if (folder == null)
            throw new ArgumentNullException(nameof(folder), "Folder cannot be null.");

        if (string.IsNullOrWhiteSpace(folder.Name) || folder.Name.Length < 3 || folder.Name.Length > 50)
            throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folder.Name));

        var existingFolder = await _folderRepository.GetByIdAsync(folder.Id);
        if (existingFolder == null)
            throw new KeyNotFoundException($"Folder with ID {folder.Id} not found.");

        existingFolder.Name = folder.Name;
        existingFolder.Notes = folder.Notes;        
        await _folderRepository.UpdateAsync(existingFolder);
        return true;
    }

    public async Task<bool> DeleteFolderAsync(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));

        var folder = await _folderRepository.GetByIdAsync(id);
        if (folder == null)
            throw new KeyNotFoundException($"Folder with ID {id} not found.");

        await _folderRepository.DeleteAsync(folder);
        return true;
    }

    public async Task<string> GetFolderNameByIdAsync(int id)
    {
        var folder = await _folderRepository.GetByIdAsync(id);
        return folder?.Name ?? string.Empty;
    }
}
