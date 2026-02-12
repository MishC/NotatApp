
using Moq;
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
        var folders = await _folderRepository.GetAllFoldersAsync();
        // Optional: sort by name
        return folders;
    }

    public async Task<Folder> GetFolderByIdAsync(int id)
    {  try {
        if (id <= 0)
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));

        var folder = await _folderRepository.GetFolderByIdAsync(id);
        if (folder == null)
            throw new KeyNotFoundException($"Folder with ID {id} not found.");

        return folder;
    }
    catch (Exception ex)
    {
        throw new Exception("An error occurred while retrieving the folder by ID.", ex);
    }
}

    public async Task<Folder?> GetFolderByNameAsync(string name)
    {  try{
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalized = name.Trim().ToLowerInvariant();
        var folders = await _folderRepository.GetAllFoldersAsync();

        return folders.FirstOrDefault(f =>
            !string.IsNullOrEmpty(f.Name) &&
            f.Name.Trim().ToLowerInvariant() == normalized);
    }
    catch (Exception ex)
    {
        throw new Exception("An error occurred while retrieving the folder by name.", ex);
    }
}

    public async Task<Folder> AddFolderAsync(CreateFolderDto folderDto, string userId)
    {
        try
        {
            if (folderDto == null)
                throw new ArgumentNullException(nameof(folderDto), "Folder cannot be null.");

            if (string.IsNullOrWhiteSpace(folderDto.Name) || folderDto.Name.Length < 1 || folderDto.Name.Length > 50)
                throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folderDto.Name));

            var folder = new Folder
            {
                Name = folderDto.Name,
                UserId = userId
            };

            await _folderRepository.AddFolderAsync(folder, userId);
            return folder;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while adding the folder.", ex);
        }
    }

    public async Task<bool> UpdateFolderAsync(int id, UpdateFolderDto folderDto, string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));
            if (folderDto?.Name == null)
                throw new ArgumentNullException(nameof(folderDto), "Folder cannot be null.");

            if (string.IsNullOrWhiteSpace(folderDto.Name) || folderDto.Name.Length < 1 || folderDto.Name.Length > 50)
                throw new ArgumentException("Folder name must be between 3 and 50 characters.", nameof(folderDto.Name));

            var existingFolder = await _folderRepository.GetFolderByIdAsync(id);
            if (existingFolder == null)
                throw new KeyNotFoundException($"Folder with ID {id} not found.");

            existingFolder.Name = folderDto.Name; // Update the folder name is the only option see FolderDto
            await _folderRepository.UpdateFolderAsync(existingFolder, userId);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while updating the folder.", ex);
        }
    }
    public async Task<bool> DeleteFolderByIdAsync(int id, string userId)
    {
        if (id <= 0)
            throw new ArgumentException("Folder ID must be greater than zero.", nameof(id));

        var existingFolder = await _folderRepository.GetFolderByIdAsync(id);
        if (existingFolder == null)
            throw new KeyNotFoundException($"Folder with ID {id} not found.");
        try
        {
            await _folderRepository.DeleteFolderByIdAsync(id, userId);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the folder.", ex);
        }
    }

    public async Task<string> GetFolderNameByIdAsync(int id)
    {
        try
        {
            var folder = await _folderRepository.GetFolderByIdAsync(id);
            return folder?.Name ?? string.Empty;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while retrieving the folder name.", ex);
        }
    }
}
