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

    public async Task<List<Folder>> GetAllFoldersAsync()
    {
        return await _context.Folders
            .Include(f => f.Notes) //this is JSON ignore
            .ToListAsync(); //returns all folders with their parameters
    }

    public async Task<Folder?> GetFolderByIdAsync(int id)
    {
        return await _context.Folders
            .Include(f => f.Notes) //this is JSON ignore -> can be deleted in final code
            .FirstOrDefaultAsync(f => f.Id == id); //Returns the folder and its notes
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
