using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Models;
using NotatApp.Services;
using NotatApp.Extensions;

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


        [HttpGet("health")]
        [AllowAnonymous]
        public ActionResult<string> HealthCheck()
                => Ok("Note Service is running.");

        // GET /api/notes
        [HttpGet]
        public async Task<ActionResult> GetAllNotes()
        {
            var notes = await _noteService.GetAllNotesAsync(User.GetUserId());
            return Ok(notes);
        }

        // GET /api/notes/pending
        [HttpGet("pending")]
        public async Task<ActionResult> GetPendingNotes()
        {
            var notes = await _noteService.GetPendingNotesAsync(User.GetUserId());
            return Ok(notes);
        }

        // GET /api/notes/done
        [HttpGet("done")]
        public async Task<ActionResult> GetDoneNotes()
        {
            var notes = await _noteService.GetDoneNotesAsync(User.GetUserId());
            return Ok(notes);
        }

        // GET /api/notes/overdues
        [HttpGet("overdues")]
        public async Task<ActionResult> GetOverdueNotes()
        {
            var notes = await _noteService.GetOverdueNotesAsync(User.GetUserId());
            return Ok(notes);
        }

        // GET /api/notes/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult> GetNoteById(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id, User.GetUserId());

            if (note is null)
                return NotFound();

            return Ok(note);
        }

        //Get All Notes by Folder Id
        //API=> GET /api/notes/folders/{folderId}
        [HttpGet("folders/{folderId:int}")]
        public async Task<IActionResult> GetNotesByFolderId(int folderId)
        {
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            var notes = await _noteService.GetNotesByFolderIdAsync(folderId, userId);
            return Ok(notes);
        }

        [HttpGet("overdue/{id:int}")]
        public async Task<ActionResult<bool>> IsOverdue(int id)
        {
            var result = await _noteService.IsNoteOverdueAsync(id, User.GetUserId());
            return Ok(result);
        }


        //Create Note
        //API=> POST /api/notes 
        // body: { "title": "Note Title", "scheduledAt": "2023-01-01T00:00:00Z", "content": "Note Content", "folderId": 1 }
        // Required: Title, scheduleAt
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteDto dto)
        {
            var userId = User.GetUserId();

            var note = await _noteService.CreateNoteAsync(dto, userId);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        //Update Note
        //API=> PUT /api/notes/{id}
        // body: { "title": "Updated Title", "scheduledAt": "2023-01-01T00:00:00Z", "content": "Updated Content", "folderId": 1 }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] UpdateNoteDto dto)
        {
            var updated = await _noteService.UpdateNoteAsync(id, dto, User.GetUserId());

            if (!updated)
                return NotFound();

            return NoContent();


        }

        [HttpPut("{folderId:int}/{id:int}")]
        public async Task<IActionResult> UpdateNoteFolder(int folderId, int id)
        {
            var userId = User.GetUserId();

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
            var userId = User.GetUserId();
            if (userId is null) return Unauthorized();

            await _noteService.SwapOrderAsync(req.SourceId, req.TargetId, userId);
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var deleted = await _noteService.DeleteNoteAsync(id, User.GetUserId());

            if (!deleted)
                return NotFound();

            return NoContent();
        }


    }
}
