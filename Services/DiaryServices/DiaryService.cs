using NotatApp.Models;
using NotatApp.Repositories.DiaryRepositories;

namespace NotatApp.Services.DiaryServices
{
    public class DiaryService(
        IDiaryRepository diaryRepository,
        IFileStorageService fileStorage) : IDiaryService
    {
        private readonly IDiaryRepository _diaryRepository = diaryRepository;
        private readonly IFileStorageService _fileStorage = fileStorage;

        public async Task<List<DiaryEntry>> GetDiaryEntriesAsync(
            string userId,
            DateOnly date
        )
        {
            return await _diaryRepository.GetByDateAsync(userId, date);
        }

        public async Task<DiaryEntry> CreateDiaryEntryAsync(
            string userId,
            CreateDiaryEntryDto dto
        )
        {
            var entry = new DiaryEntry
            {
                Title = dto.Title,
                Content = dto.Content,
                Date = dto.Date,
                UserId = userId
            };

            if (dto.Image != null)
            {
                var stored = await _fileStorage.SaveDiaryImageAsync(dto.Image, userId);

                entry.ImagePath = stored.ImagePath;
                entry.ImageContentType = stored.ImageContentType;
                entry.ImageFileName = stored.ImageFileName;
            }

            await _diaryRepository.AddAsync(entry);
            await _diaryRepository.SaveChangesAsync();

            return entry;
        }

        public async Task<bool> UpdateDiaryEntryAsync(
            int id,
            UpdateDiaryEntryDto dto,
            string userId
        )
        {
            var entry = await _diaryRepository.GetByIdAsync(id, userId);

            if (entry == null)
                return false;

            if (dto.Title != null)
                entry.Title = dto.Title;

            if (dto.Content != null)
                entry.Content = dto.Content;

            if (dto.Date.HasValue)
                entry.Date = dto.Date.Value;

            if (dto.RemoveImage)
            {
                await _fileStorage.DeleteFileAsync(entry.ImagePath);

                entry.ImagePath = null;
                entry.ImageContentType = null;
                entry.ImageFileName = null;
            }

            if (dto.Image != null)
            {
                await _fileStorage.DeleteFileAsync(entry.ImagePath);

                var stored = await _fileStorage.SaveDiaryImageAsync(dto.Image, userId);

                entry.ImagePath = stored.ImagePath;
                entry.ImageContentType = stored.ImageContentType;
                entry.ImageFileName = stored.ImageFileName;
            }

            await _diaryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<(string absolutePath, string contentType)?> GetDiaryImageAsync(
            int id,
            string userId
        )
        {
            var entry = await _diaryRepository.GetByIdAsync(id, userId);

            if (entry == null)
                return null;

            if (string.IsNullOrWhiteSpace(entry.ImagePath) ||
                string.IsNullOrWhiteSpace(entry.ImageContentType))
                return null;

            var absolutePath = _fileStorage.GetAbsolutePath(entry.ImagePath);

            if (!File.Exists(absolutePath))
                return null;

            return (absolutePath, entry.ImageContentType);
        }
    }
}