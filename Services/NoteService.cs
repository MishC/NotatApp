using System.Xml.Serialization;
using Microsoft.AspNetCore.Identity;
using NotatApp.Models;
using NotatApp.Repositories;



namespace NotatApp.Services
{
    public class NoteService : INoteService
    {
        private readonly INoteRepository _repository;

        public NoteService(INoteRepository repository)
        {
            _repository = repository;
        }

        // All
        public async Task<List<Note>> GetAllNotesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var notes = await _repository.GetUserNotesAsync(userId);

            return [.. notes.OrderBy(n => n.OrderIndex)];
        }

        // Pending 
        public async Task<List<Note>> GetPendingNotesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var notes = await _repository.GetUserNotesAsync(userId);

            return [.. notes
                .Where(n => !n.IsDone && ( n.Folder?.Name != "Done"))
                .OrderBy(n => n.OrderIndex)];
        }

        // Done 
        public async Task<List<Note>> GetDoneNotesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var notes = await _repository.GetUserNotesAsync(userId);

            return [.. notes
                .Where(n => n.Folder != null && n.Folder.Name == "Done")
                .OrderBy(n => n.OrderIndex)];
        }

        public async Task<List<Note>> GetOverdueNotesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var notes = await _repository.GetUserNotesAsync(userId);
            var today = DateOnly.FromDateTime(DateTime.Now);


            return [.. notes
                .Where(n => n.ScheduledAt.HasValue && n.ScheduledAt.Value < today)
                .OrderBy(n => n.OrderIndex)];
        }

        // Get one note 
        public Task<Note?> GetNoteByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            return _repository.GetNoteByIdAsync(id, userId);
        }

        public Task<List<Note?>> GetNotesByFolderIdAsync(int folderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            return _repository.GetNotesByFolderIdAsync(folderId, userId);
        }

        //Create Note
        public async Task<Note> CreateNoteAsync(CreateNoteDto dto, string userId)
        {
            if (dto is null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("UserId is required.", nameof(userId));

            if (string.IsNullOrWhiteSpace(dto.Title))
                throw new ArgumentException("Title is required.", nameof(dto.Title));

            if (dto.Title.Length > 100)
                throw new ArgumentException("Title cannot be longer than 100 characters.", nameof(dto.Title));

            if (dto.ScheduledAt.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (dto.ScheduledAt.Value < today)
                    throw new ArgumentException("Deadline must be today or in the future");

            }


            var nextIndex = await _repository.GetNextOrderIndexAsync(userId);

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
                FolderId = dto.FolderId,
                UserId = userId,
                IsDone = false,
                OrderIndex = nextIndex,
                ScheduledAt = dto.ScheduledAt
            };



            await _repository.AddNoteAsync(note);
            return note;
        }

        public async Task<bool> UpdateNoteAsync(int id, UpdateNoteDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var note = await _repository.GetNoteByIdAsync(id, userId);
            if (note == null)
                return false;

            if (dto.Title != null)
                note.Title = dto.Title;

            if (dto.Content != null)
                note.Content = dto.Content;

            if (dto.FolderId.HasValue)
                note.FolderId = dto.FolderId;


            if (dto.ScheduledAt.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                if (dto.ScheduledAt.Value < today)
                    note.ScheduledAt = note.ScheduledAt;

                note.ScheduledAt = dto.ScheduledAt;
            }
            try
            {
                await _repository.UpdateNoteAsync(note);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the note.", ex);
            }
        }

        public async Task<bool> UpdateNoteFolderAsync(int id, int folderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var note = await _repository.GetNoteByIdAsync(id, userId);
            if (note == null)
                return false;

            note.FolderId = folderId;

            await _repository.UpdateNoteAsync(note);
            return true;
        }

        public async Task<bool> DeleteNoteAsync(int id, string userId)
        {  try{
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var existing = await _repository.GetNoteByIdAsync(id, userId);
            if (existing == null)
                return false;

            await _repository.DeleteNoteAsync(existing);
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred while deleting the note.", ex);
        }
    }

        public async Task SwapOrderAsync(int sourceId, int targetId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var source = await _repository.GetNoteByIdAsync(sourceId, userId);
            var target = await _repository.GetNoteByIdAsync(targetId, userId);

            if (source == null || target == null)
                return;

            var tmp = source.OrderIndex;
            source.OrderIndex = target.OrderIndex;
            target.OrderIndex = tmp;

            await _repository.UpdateNoteAsync(source);
            await _repository.UpdateNoteAsync(target);
        }


        public async Task<bool> IsNoteOverdueAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var note = await _repository.GetNoteByIdAsync(id, userId);
            if (note == null)
                return false;

            var today = DateOnly.FromDateTime(DateTime.Now);
            return note.ScheduledAt.HasValue && note.ScheduledAt.Value < today;
        }

    }
}
