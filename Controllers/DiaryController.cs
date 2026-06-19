using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using NotatApp.Models;
using NotatApp.Services.DiaryServices;

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/diary")]
    [Authorize]
    public class DiaryController : ControllerBase
    {
        private readonly IDiaryService _diaryService;

        public DiaryController(IDiaryService diaryService)
        {
            _diaryService = diaryService;
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiaryEntries([FromQuery] DateOnly date)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var entries = await _diaryService.GetDiaryEntriesAsync(userId, date);

            var result = entries.Select(e => new
            {
                e.Id,
                e.Title,
                e.Content,
                e.Date,
                HasImage = e.ImagePath != null,
                e.ImageFileName
            });

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateDiaryEntry([FromForm] CreateDiaryEntryDto dto)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var entry = await _diaryService.CreateDiaryEntryAsync(userId, dto);

                return Ok(new
                {
                    entry.Id,
                    entry.Title,
                    entry.Content,
                    entry.Date,
                    HasImage = entry.ImagePath != null,
                    entry.ImageFileName
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateDiaryEntry(
            int id,
            [FromForm] UpdateDiaryEntryDto dto
        )
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var ok = await _diaryService.UpdateDiaryEntryAsync(id, dto, userId);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/image")]
        public async Task<IActionResult> GetDiaryImage(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var image = await _diaryService.GetDiaryImageAsync(id, userId);

            if (image == null)
                return NotFound();

            return PhysicalFile(image.Value.absolutePath, image.Value.contentType);
        }
    }
}