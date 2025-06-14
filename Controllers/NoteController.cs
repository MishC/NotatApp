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
        public async Task<IActionResult> UpdateNote(int id, Note note)
        {
            note.Id = id;
            await _noteService.UpdateNoteAsync(note);
            return NoContent();
        }
         
        [HttpPut("{folderId}/{id}")]
        public async Task<IActionResult> UpdateNoteFolder(int folderId, int id)
        {
            var note = await _noteService.GetNoteByIdAsync(id);
            if (note == null)
            return NotFound();

            note.FolderId = folderId;
            await _noteService.UpdateNoteAsync(note);
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
