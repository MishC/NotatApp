using Microsoft.EntityFrameworkCore;

using NotatApp.Models;
using NotatApp.Data;

public class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _context;

    public FolderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Folder>> GetAllFoldersAsync(string? userId)
    {
        return await _context.Folders
            .Include(f => f.Notes)
            .Where(f => f.UserId == null || f.UserId == userId)
            .ToListAsync();
    }

    public async Task<Folder?> GetFolderByIdAsync(int id)
    {
        return await _context.Folders
            .Include(f => f.Notes) //this is JSON ignore -> can be deleted in final code
            .FirstOrDefaultAsync(f => f.Id == id); //Returns the folder and its notes
    }
    public async Task<Folder?> GetFolderByNameAsync(string name)
    {
        var normalized = name.Trim().ToLowerInvariant();

        return await _context.Folders
            .Include(f => f.Notes)
            .FirstOrDefaultAsync(f =>
                f.Name != null &&
                f.Name.ToLower() == normalized);
    }



    public async Task AddFolderAsync(Folder folder, string userId)
    {
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFolderAsync(Folder folder, string userId)
    {
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFolderAsync(Folder folder, string userId)
    {
        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFolderByIdAsync(int id, string userId)
    {
        var folder = await GetFolderByIdAsync(id);
        if (folder != null)
        {
            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();
        }
    }
}
