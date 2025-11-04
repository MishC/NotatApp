using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;
using NotatApp.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

public class FolderRepository : IFolderRepository
{
    private readonly ApplicationDbContext _context;

    public FolderRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Folder>> GetAllFoldersAsync()
    {
        return await _context.Folders.Include(f => f.Notes).ToListAsync();
    }

    public async Task<Folder> GetFolderByIdAsync(int id)
    {
        var folder = await _context.Folders.Include(f => f.Notes).FirstOrDefaultAsync(f => f.Id == id) ?? throw new KeyNotFoundException($"Folder with ID {id} not found.");
        return folder;
    }
    
    
    public async Task<Folder> AddFolderAsync(Folder folder)
    {
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();
        return folder;
    }

    public async Task<bool> UpdateFolderAsync(Folder folder)
    {
        _context.Folders.Update(folder);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteFolderAsync(int id)
    {
        var folder = await _context.Folders.FindAsync(id);
        if (folder == null) return false;

        _context.Folders.Remove(folder);
        return await _context.SaveChangesAsync() > 0;
    }

 public async Task<Folder?> GetFolderByNameAsync(string name)
{
    // Normalize for case-insensitive comparison
    var normalized = name.Trim().ToLowerInvariant();
    return await _context.Folders
        .Include(f => f.Notes)
        .FirstOrDefaultAsync(f => f.Name!.ToLower() == normalized);
}
   
}
