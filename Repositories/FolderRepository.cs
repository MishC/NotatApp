using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            .Include(f => f.Notes)
            .ToListAsync();
    }

    public async Task<Folder?> GetFolderByIdAsync(int id)
    {
        return await _context.Folders
            .Include(f => f.Notes)
            .FirstOrDefaultAsync(f => f.Id == id); //Returns the folder and its notes
    }

    public async Task AddFolderAsync(Folder folder)
    {
        _context.Folders.Add(folder);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateFolderAsync(Folder folder)
    {
        _context.Folders.Update(folder);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFolderAsync(Folder folder)
    {
        _context.Folders.Remove(folder);
        await _context.SaveChangesAsync();
    }
}
