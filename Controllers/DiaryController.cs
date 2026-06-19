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

        private static object ToPageResponse(DiaryPage page)
        {
            return new
            {
                page.Id,
                page.PageNumber,
                page.Content,
                HasImage = page.ImagePath != null,
                page.ImageFileName,
                page.CreatedAt,
                page.UpdatedAt,
                page.ImageUploadedAt
            };
        }

        private static object ToEntryResponse(DiaryEntry entry)
        {
            return new
            {
                entry.Id,
                entry.Title,
                entry.Date,
                entry.CreatedAt,
                entry.UpdatedAt,
                Pages = entry.Pages
                    .OrderBy(p => p.PageNumber)
                    .Select(ToPageResponse)
            };
        }

        private IActionResult InvalidOperationResult(InvalidOperationException ex)
        {
            var body = new { message = ex.Message };
            return ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                ? Conflict(body)
                : BadRequest(body);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiaryEntries([FromQuery] DateOnly date)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var entries = await _diaryService.GetDiaryEntriesAsync(userId, date);

            var result = entries.Select(ToEntryResponse);

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

                return Ok(ToEntryResponse(entry));
            }
            catch (InvalidOperationException ex)
            {
                return InvalidOperationResult(ex);
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
                return InvalidOperationResult(ex);
            }
        }

        [HttpPost("{entryId:int}/pages")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateDiaryPage(
            int entryId,
            [FromForm] CreateDiaryPageDto dto
        )
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var page = await _diaryService.CreateDiaryPageAsync(entryId, dto, userId);
                return page == null ? NotFound() : Ok(ToPageResponse(page));
            }
            catch (InvalidOperationException ex)
            {
                return InvalidOperationResult(ex);
            }
        }

        [HttpPut("pages/{pageId:int}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateDiaryPage(
            int pageId,
            [FromForm] UpdateDiaryPageDto dto
        )
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            try
            {
                var ok = await _diaryService.UpdateDiaryPageAsync(pageId, dto, userId);
                return ok ? NoContent() : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return InvalidOperationResult(ex);
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

        [HttpGet("pages/{pageId:int}/image")]
        public async Task<IActionResult> GetDiaryPageImage(int pageId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var image = await _diaryService.GetDiaryPageImageAsync(pageId, userId);

            if (image == null)
                return NotFound();

            return PhysicalFile(image.Value.absolutePath, image.Value.contentType);
        }

        [HttpDelete("pages/{pageId:int}")]
        public async Task<IActionResult> DeleteDiaryPage(int pageId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var deleted = await _diaryService.DeleteDiaryPageAsync(pageId, userId);
            return deleted ? NoContent() : NotFound();
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
