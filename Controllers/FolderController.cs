using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

     private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

   
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetFolders()
    {
        return Ok(await _folderService.GetAllFoldersAsync());
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFolder(int id)
    {
        var folder = await _folderService.GetFolderByIdAsync(id);
        return folder == null ? NotFound() : Ok(folder);
    }

   
    [HttpGet("title/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFolderTitle(int id)
    {
        var title = await _folderService.GetFolderNameByIdAsync(id);
        return title == null ? NotFound() : Ok(title);
    }

    [HttpPost]
    public async Task<IActionResult> CreateFolder([FromBody] Folder folder)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _folderService.AddFolderAsync(folder, userId);
        return CreatedAtAction(nameof(GetFolder), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateFolder(int id, [FromBody] Folder folder)
    {

        var userId = GetUserId();
            if (userId is null) return Unauthorized();
        
        if (id != folder.Id)
            return BadRequest("Route ID does not match folder.Id");

        var updated = await _folderService.UpdateFolderAsync(folder, userId);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFolder(int id, string userId)
    {
        var deleted = await _folderService.DeleteFolderByIdAsync(id, userId);
        return deleted ? NoContent() : NotFound();
    }
}

}
