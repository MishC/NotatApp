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
            var notes = await _noteRepository.GetAllNotesAsync();
            return notes.OrderBy(n => n.OrderIndex).ToList();

        }
        // NoteService.cs
        public async Task<IEnumerable<Note>> GetPendingNotesAsync()
        {
            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            var allNotes = await _noteRepository.GetAllNotesAsync();
            return allNotes.Where(n => n.FolderId != 4).OrderBy(n => n.OrderIndex);
        }

        public async Task<IEnumerable<Note>> GetDoneNotesAsync()
        {
            if (_noteRepository == null)
            {
                throw new ArgumentNullException(nameof(_noteRepository));
            }
            var allNotes = await _noteRepository.GetAllNotesAsync();
            return allNotes.Where(n => n.FolderId == 4).OrderBy(n => n.OrderIndex);
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


            if (note == null)
            {
                throw new ArgumentNullException(nameof(note), "Note cannot be null.");
            }

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
            var allNotes = await _noteRepository.GetAllNotesAsync();
            var maxIndex = allNotes
                .Where(n => n.FolderId == note?.FolderId)
                .Select(n => (int?)n.OrderIndex)
                .DefaultIfEmpty(-1)
                .Max() ?? -1;

            if (note != null)
            {
                note.OrderIndex = maxIndex + 1;
                await _noteRepository.AddNoteAsync(note);
            }
            else
            {
                throw new ArgumentNullException(nameof(note), "Note cannot be null.");
            }
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

        public async Task SwapOrderAsync(int sourceId, int targetId)
        {
            var a = await _noteRepository.GetNoteByIdAsync(sourceId)
            ?? throw new KeyNotFoundException($"Note {sourceId} not found");
            var b = await _noteRepository.GetNoteByIdAsync(targetId)
                ?? throw new KeyNotFoundException($"Note {targetId} not found");

            // swap
            (a.OrderIndex, b.OrderIndex) = (b.OrderIndex, a.OrderIndex);

        await _noteRepository.UpdateNoteAsync(a);
        await _noteRepository.UpdateNoteAsync(b);

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
