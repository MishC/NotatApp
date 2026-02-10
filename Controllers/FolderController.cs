using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NotatApp.Models;

namespace NotatApp.Controllers
{
[ApiController]
[Route("api/folders")]
[Authorize]
public class FolderController : ControllerBase
{
    private readonly IFolderService _folderService;

    public FolderController(IFolderService folderService)
    {
        _folderService = folderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetFolders()
    {
        return Ok(await _folderService.GetAllFoldersAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFolder(int id)
    {
        var folder = await _folderService.GetFolderByIdAsync(id);
        return folder == null ? NotFound() : Ok(folder);
    }

    [HttpGet("{id:int}/notes")]
    public async Task<IActionResult> GetNotesForFolder(int id)
    {
        var notes = await _folderService.GetNotesForFolderAsync(id);
        return notes == null ? NotFound() : Ok(notes);
    }
    [HttpGet("title/{id:int}")]
    public async Task<IActionResult> GetFolderTitle(int id)
    {
        var title = await _folderService.GetFolderNameByIdAsync(id);
        return title == null ? NotFound() : Ok(title);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateFolder([FromBody] Folder folder)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _folderService.AddFolderAsync(folder);
        return CreatedAtAction(nameof(GetFolder), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFolder(int id, [FromBody] Folder folder)
    {
        if (id != folder.Id)
            return BadRequest("Route ID does not match folder.Id");

        var updated = await _folderService.UpdateFolderAsync(folder);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFolder(int id)
    {
        var deleted = await _folderService.DeleteFolderAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

}
