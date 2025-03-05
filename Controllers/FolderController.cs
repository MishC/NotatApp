using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using NotatApp.Models;

[Route("api/folders")]
[ApiController]
public class FolderController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FolderController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    // Get all folders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Folder>>> GetFolders()
    {
        return Ok(await _folderService.GetAllFoldersAsync());
    }

    // Get folder by id
    [HttpGet("{id}")]
    public async Task<ActionResult<Folder>> GetFolder(int id)
    {
        var folder = await _folderService.GetFolderByIdAsync(id);
        if (folder == null) return NotFound();
        return Ok(folder);
    }

    // Create folder
    [HttpPost]
    public async Task<ActionResult<Folder>> CreateFolder(Folder folder)
    {
        if (string.IsNullOrWhiteSpace(folder.Name) || folder.Name.Length < 3)
        {
            return BadRequest("Folder name must be at least 3 characters long.");
        }

        var createdFolder = await _folderService.AddFolderAsync(folder);
        return CreatedAtAction(nameof(GetFolder), new { id = createdFolder.Id }, createdFolder);
    }

    // Update folder
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFolder(int id, Folder folder)
    {
        if (id != folder.Id) return BadRequest();

        var success = await _folderService.UpdateFolderAsync(folder);
        if (!success) return NotFound();
        return NoContent();
    }

    // Delete folder
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        var success = await _folderService.DeleteFolderAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}
