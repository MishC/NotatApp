using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NotatApp.Models;
using NotatApp.Extensions;

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
        [AllowAnonymous]
        public async Task<IActionResult> GetFolders()
        {
            var userId = User.GetUserId();
            return Ok(await _folderService.GetAllFoldersAsync(userId));
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFolder(int id)
        {
            var folder = await _folderService.GetFolderByIdAsync(id, User.GetUserId());
            return folder == null ? NotFound() : Ok(folder);
        }


        [HttpGet("title/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFolderTitle(int id)
        {
            var title = await _folderService.GetFolderNameByIdAsync(id, User.GetUserId());
            return title == null ? NotFound() : Ok(title);
        }

        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] CreateFolderDto dto)
        {
            var userId = User.GetUserId();

            var created = await _folderService.AddFolderAsync(dto, userId);
            return CreatedAtAction(nameof(GetFolder), new { id = created.Id }, created);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateFolder(int id, [FromBody] UpdateFolderDto dto)
        {
            var userId = User.GetUserId();

            var ok = await _folderService.UpdateFolderAsync(id, dto, userId);
            return ok ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var userId = User.GetUserId();

            var ok = await _folderService.DeleteFolderByIdAsync(id, userId);
            return ok ? NoContent() : NotFound();
        }

    }

}
