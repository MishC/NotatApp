using Microsoft.AspNetCore.Authorization;
using NotatApp.Models;
using NotatApp.Repositories;

namespace NotatApp.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _noteRepository;

        public NoteService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<List<Note>> GetAllNotesAsync()
        {
            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            return await _noteRepository.GetAllNotesAsync();

        }
        // NoteService.cs
        public async Task<IEnumerable<Note>> GetPendingNotesAsync()
        {
            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            var allNotes = await _noteRepository.GetAllNotesAsync();
            return allNotes.Where(n => n.FolderId != 4);
        }

        public async Task<IEnumerable<Note>> GetDoneNotesAsync()
        {
            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            var allNotes = await _noteRepository.GetAllNotesAsync();
            return allNotes.Where(n => n.FolderId == 4);
        }

        public async Task<Note?> GetNoteByIdAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be greater than 0", nameof(id));
            }

            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            var note = await _noteRepository.GetNoteByIdAsync(id);

            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {id} not found.");
            }

            return note;


        }
        public async Task AddNoteAsync(Note note)
        {


            ArgumentNullException.ThrowIfNull(note);

            if (string.IsNullOrWhiteSpace(note.Title))

            {
                throw new ArgumentException("Title cannot be null or empty.");
            }
            if (note?.Content?.Length > 1000)
            {
                throw new ArgumentException("Content cannot exceed 1000 characters.");
            }

            if (note?.Title.Length < 1 || note?.Title.Length > 100)
            {
                throw new ArgumentException("Title must have at least 1 character. Title cannot exceed 100 characters.");
            }
            await _noteRepository.AddNoteAsync(note!);
        }
  public async Task UpdateNoteAsync(Note note)
{
    var existingNote = await _noteRepository.GetNoteByIdAsync(note.Id) 
        ?? throw new KeyNotFoundException($"Note with ID {note.Id} not found.");

    if (string.IsNullOrWhiteSpace(note.Title) || note.Title.Length > 100)
    {
        throw new ArgumentException("Title must be between 1 and 100 characters.");
    }

    existingNote.Title = note.Title;
    existingNote.Content = note.Content;
    existingNote.FolderId = note.FolderId;

    await _noteRepository.UpdateNoteAsync(existingNote);
}

        public async Task DeleteNoteAsync(int id)
        {

            var note = await _noteRepository.GetNoteByIdAsync(id);
            if (id <= 0)
            {
                throw new ArgumentException("Id must be greater than 0", nameof(id));
            }

            if (note == null)
            {
                throw new KeyNotFoundException($"Note with ID {id} not found.");
            }


            await _noteRepository.DeleteNoteAsync(id);
        }

    }
}
