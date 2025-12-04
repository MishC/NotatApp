using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Models;
using NotatApp.Services;

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/notes")]
    [Authorize] 
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        private string? GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var notes = await _noteService.GetAllNotesAsync(userId);
            return Ok(notes);
        }

        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck() => Ok("Note Service is running.");

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var pending = await _noteService.GetPendingNotesAsync(userId);
            return Ok(pending);
        }

        [HttpGet("done")]
        public async Task<IActionResult> GetDoneNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var done = await _noteService.GetDoneNotesAsync(userId);
            return Ok(done);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var note = await _noteService.GetNoteByIdAsync(id, userId);
            return note == null ? NotFound() : Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto dto)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var note = await _noteService.CreateNoteAsync(dto, userId);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updated = await _noteService.UpdateNoteAsync(id, dto, userId);
            if (!updated) return NotFound();

            return NoContent();
        }

        [HttpPut("{folderId:int}/{id:int}")]
        public async Task<IActionResult> UpdateNoteFolder(int folderId, int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var updated = await _noteService.UpdateNoteFolderAsync(id, folderId, userId);
            if (!updated) return NotFound();

            return NoContent();
        }

        public class SwapRequest
        {
            public int SourceId { get; set; }
            public int TargetId { get; set; }
        }

        [HttpPost("swap")]
        public async Task<IActionResult> Swap([FromBody] SwapRequest req)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            await _noteService.SwapOrderAsync(req.SourceId, req.TargetId, userId);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var deleted = await _noteService.DeleteNoteAsync(id, userId);
            if (!deleted) return NotFound();

            return NoContent();
        }
    }
}
