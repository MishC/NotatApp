using Microsoft.AspNetCore.Mvc;
using NotatApp.Models;
using NotatApp.Services;

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NoteController : ControllerBase
    {
        private readonly INoteService _noteService;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllNotes() => Ok(await _noteService.GetAllNotesAsync());

         [HttpGet("health")]
        public IActionResult HealthCheck() => Ok("Note Service is running.");

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingNotes()
        {
            var pending = await _noteService.GetPendingNotesAsync();
            return Ok(pending);
        }
        [HttpGet("done")]
        public async Task<IActionResult> GetDoneNotes()
        {
            var pending = await _noteService.GetDoneNotesAsync();
            return Ok(pending);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            return note == null ? NotFound() : Ok(note);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNote(Note note)
        {

            await _noteService.AddNoteAsync(note);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] Note note)
        {
            if (note.Id != id)
            {
                return BadRequest("Note ID in the body does not match the ID in the URL.");
            }

            await _noteService.UpdateNoteAsync(note);
            return NoContent();
        }
        [HttpPut("{folderId}/{id}")]
        public async Task<IActionResult> UpdateNoteFolder(int folderId, int id, [FromBody] Note updatedNote)
        {
            if (updatedNote == null || updatedNote.Id != id)
                return BadRequest("Invalid note data.");


            await _noteService.UpdateNoteAsync(updatedNote);
            return NoContent();
        }

        // Swap two notes (fast path)
        [HttpPost("swap")]
        public async Task<IActionResult> Swap(int source, int target)
        {

            await _noteService.SwapOrderAsync(source, target);
            return NoContent();
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            await _noteService.DeleteNoteAsync(id);
            return NoContent();
        }
    }
}
