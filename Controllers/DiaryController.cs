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

            var result = entries.Select(e =>
            {
                var page = e.Pages.OrderBy(p => p.PageNumber).FirstOrDefault();

                return new
                {
                    e.Id,
                    e.Title,
                    Content = page?.Content,
                    e.Date,
                    PageNumber = page?.PageNumber,
                    HasImage = page?.ImagePath != null,
                    page?.ImageFileName
                };
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
                    Content = entry.Pages.FirstOrDefault()?.Content,
                    entry.Date,
                    PageNumber = entry.Pages.FirstOrDefault()?.PageNumber,
                    HasImage = entry.Pages.FirstOrDefault()?.ImagePath != null,
                    ImageFileName = entry.Pages.FirstOrDefault()?.ImageFileName
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


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteDiaryEntryById(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var deleted = await _diaryService.DeleteDiaryEntryByIdAsync(id, userId);
            return deleted ? NoContent() : NotFound();
        }

        [HttpDelete("date/{date}")]
        public async Task<IActionResult> DeleteDiaryEntriesByDate(DateOnly date)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();
            var deleted = await _diaryService.DeleteDiaryEntriesByDateAsync(userId, date);
            return deleted ? NoContent() : NotFound();
        }

        }
}
