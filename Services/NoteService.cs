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
                .Where(n => !n.IsArchived && ( n.Folder?.Name != "Done"))
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


        // Get one note 
        public Task<Note?> GetNoteByIdAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            return _repository.GetByIdAsync(id, userId);
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


            var nextIndex = await _repository.GetNextOrderIndexAsync(userId);

            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
                FolderId = dto.FolderId,
                UserId = userId,
                IsArchived = false,
                OrderIndex = nextIndex,
                ScheduledAt = dto.ScheduledAt
            };

            await _repository.AddAsync(note);
            return note;
        }

        public async Task<bool> UpdateNoteAsync(int id, UpdateNoteDto dto, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var note = await _repository.GetByIdAsync(id, userId);
            if (note == null)
                return false;

            note.Title = dto.Title;
            note.Content = dto.Content;
            note.FolderId = dto.FolderId;
            note.IsArchived = dto.IsDone;
            note.ScheduledAt = dto.ScheduledAt;

            await _repository.UpdateAsync(note);
            return true;
        }

        public async Task<bool> UpdateNoteFolderAsync(int id, int folderId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var note = await _repository.GetByIdAsync(id, userId);
            if (note == null)
                return false;

            note.FolderId = folderId;
            await _repository.UpdateAsync(note);
            return true;
        }

        public async Task<bool> DeleteNoteAsync(int id, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var existing = await _repository.GetByIdAsync(id, userId);
            if (existing == null)
                return false;

            await _repository.DeleteAsync(existing);
            return true;
        }

        public async Task SwapOrderAsync(int sourceId, int targetId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required", nameof(userId));

            var source = await _repository.GetByIdAsync(sourceId, userId);
            var target = await _repository.GetByIdAsync(targetId, userId);

            if (source == null || target == null)
                return;

            var tmp = source.OrderIndex;
            source.OrderIndex = target.OrderIndex;
            target.OrderIndex = tmp;

            await _repository.UpdateAsync(source);
            await _repository.UpdateAsync(target);
        }
    }
}
