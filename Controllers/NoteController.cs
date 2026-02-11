using System.IdentityModel.Tokens.Jwt;
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
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);


        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult HealthCheck() => Ok("Note Service is running.");

        //Get All notes 
        //API=> GET /api/notes
        [HttpGet]
        public async Task<IActionResult> GetAllNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var notes = await _noteService.GetAllNotesAsync(userId);
            return Ok(notes);
        }

        //Get Pending notes
        //API=> GET /api/notes/pending
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var notes = await _noteService.GetPendingNotesAsync(userId);
            return Ok(notes);
        }

        //Get Done notes
        //API=> GET /api/notes/done
        [HttpGet("done")]
        public async Task<IActionResult> GetDoneNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var done = await _noteService.GetDoneNotesAsync(userId);
            return Ok(done);
        }

        //Get Overdue notes
        //API=> GET /api/notes/overdues
        [HttpGet("overdues")]
        public async Task<IActionResult> GetOverdueNotes()
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var overdue = await _noteService.GetOverdueNotesAsync(userId);
            return Ok(overdue);
        }

        //Get Note by Id
        //API=> GET /api/notes/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var note = await _noteService.GetNoteByIdAsync(id, userId);
            return note == null ? NotFound() : Ok(note);
        }


        //Get All Notes by Folder Id
        //API=> GET /api/notes/folders/{folderId}
        [HttpGet("folders/{folderId:int}")]
        public async Task<IActionResult> GetNotesByFolderId(int folderId)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var notes = await _noteService.GetNotesByFolderIdAsync(folderId, userId);
            return Ok(notes);
        }


        //Create Note
        //API=> POST /api/notes 
        // body: { "title": "Note Title", "scheduledAt": "2023-01-01T00:00:00Z", "content": "Note Content", "folderId": 1 }
        // Required: Title, scheduleAt
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto dto)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var note = await _noteService.CreateNoteAsync(dto, userId);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        //Update Note
        //API=> PUT /api/notes/{id}
        // body: { "title": "Updated Title", "scheduledAt": "2023-01-01T00:00:00Z", "content": "Updated Content", "folderId": 1 }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

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

        [HttpGet("overdue/{id:int}")]
        public async Task<IActionResult> IsOverdue(int id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var isOverdue = await _noteService.IsNoteOverdueAsync(id, userId);
            return Ok(isOverdue);
        }
   
    }
}
