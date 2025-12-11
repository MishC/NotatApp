using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

public interface IFolderRepository
{
    Task<List<Folder>> GetAllAsync();
    Task<Folder?> GetByIdAsync(int id);

    Task AddAsync(Folder folder);
    Task UpdateAsync(Folder folder);
    Task DeleteAsync(Folder folder);
}
