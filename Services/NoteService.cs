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

        public async Task<IReadOnlyList<Note>> GetAllNotesAsync(string userId) =>
            await _repository.GetAllNotesAsync(userId);

        public async Task<IReadOnlyList<Note>> GetPendingNotesAsync(string userId) =>
            await _repository.GetPendingNotesAsync(userId);

        public async Task<IReadOnlyList<Note>> GetDoneNotesAsync(string userId) =>
            await _repository.GetDoneNotesAsync(userId);

        public Task<Note?> GetNoteByIdAsync(int id, string userId) =>
            _repository.GetNoteByIdAsync(id, userId);

        public async Task<Note> CreateNoteAsync(CreateNoteDto dto, string userId)
        {
            var note = new Note
            {
                Title = dto.Title,
                Content = dto.Content,
                FolderId = dto.FolderId,
                UserId = userId,
                IsArchived = false,
                OrderIndex = 0 // alebo nejaké max+1, podľa tvojej logiky
            };

            await _repository.AddNoteAsync(note);
            return note;
        }

        public async Task<bool> UpdateNoteAsync(int id, UpdateNoteDto dto, string userId)
        {
            var note = await _repository.GetNoteByIdAsync(id, userId);
            if (note == null)
                return false;

            note.Title = dto.Title;
            note.Content = dto.Content;
            note.FolderId = dto.FolderId;
            note.IsArchived = dto.IsDone;

            await _repository.UpdateNoteAsync(note);
            return true;
        }

        public async Task<bool> UpdateNoteFolderAsync(int id, int folderId, string userId)
        {
            var note = await _repository.GetNoteByIdAsync(id, userId);
            if (note == null)
                return false;

            note.FolderId = folderId;
            await _repository.UpdateNoteAsync(note);
            return true;
        }

        public async Task<bool> DeleteNoteAsync(int id, string userId)
        {
            var existing = await _repository.GetNoteByIdAsync(id, userId);
            if (existing == null)
                return false;

            await _repository.DeleteNoteAsync(id, userId);
            return true;
        }

        public Task SwapOrderAsync(int sourceId, int targetId, string userId) =>
            _repository.SwapOrderAsync(sourceId, targetId, userId);
    }
}
