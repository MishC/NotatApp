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
public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
{
    var userId = GetUserId();
    if (userId is null) return Unauthorized();
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var created = await _folderService.AddFolderAsync(dto, userId);
    return CreatedAtAction(nameof(GetFolder), new { id = created.Id }, created);
}

[HttpPut("{id:int}")]
public async Task<IActionResult> UpdateFolder(int id, [FromBody] UpdateFolderDto dto)
{
    var userId = GetUserId();
    if (userId is null) return Unauthorized();
    if (!ModelState.IsValid) return BadRequest(ModelState);

    var ok = await _folderService.UpdateFolderAsync(id, dto, userId);
    return ok ? NoContent() : NotFound();
}

[HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteFolder(int id)
{
    var userId = GetUserId();
    if (userId is null) return Unauthorized();

    var ok = await _folderService.DeleteFolderByIdAsync(id, userId);
    return ok ? NoContent() : NotFound();
}

}

}
