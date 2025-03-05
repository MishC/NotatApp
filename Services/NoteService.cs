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
            return await _noteRepository.GetAllNotesAsync();

        }
    public async Task<Note?> GetNoteByIdAsync(int id) 
    {   
        if (id <= 0)
        {
            throw new ArgumentException("Id must be greater than 0", nameof(id));
        }
        return await _noteRepository.GetNoteByIdAsync(id);

    }
        public async Task AddNoteAsync(Note note) 
        {
            if (note == null)
            {
                throw new ArgumentNullException(nameof(note));
            }   
           await _noteRepository.AddNoteAsync(note);
        
        }
        public async Task UpdateNoteAsync(Note note)
        {
            if (note == null)
            {
                throw new ArgumentNullException(nameof(note));
            }
            await _noteRepository.UpdateNoteAsync(note);
        }
        public async Task DeleteNoteAsync(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be greater than 0", nameof(id));
            }
            await _noteRepository.DeleteNoteAsync(id);
        }   
      
    }
}
