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
            var page = new DiaryPage
            {
                PageNumber = 1,
                Content = dto.Content
            };

            var entry = new DiaryEntry
            {
                Title = dto.Title,
                Date = dto.Date,
                UserId = userId,
                Pages = [page]
            };

            if (dto.Image != null)
            {
                var stored = await _fileStorage.SaveDiaryImageAsync(dto.Image, userId);

                page.ImagePath = stored.ImagePath;
                page.ImageContentType = stored.ImageContentType;
                page.ImageFileName = stored.ImageFileName;
                page.ImageUploadedAt = DateTime.UtcNow;
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

            if (dto.Date.HasValue)
                entry.Date = dto.Date.Value;

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

            var page = entry.Pages.OrderBy(p => p.PageNumber).FirstOrDefault();

            if (page == null ||
                string.IsNullOrWhiteSpace(page.ImagePath) ||
                string.IsNullOrWhiteSpace(page.ImageContentType))
                return null;

            var absolutePath = _fileStorage.GetAbsolutePath(page.ImagePath);

            if (!File.Exists(absolutePath))
                return null;

            return (absolutePath, page.ImageContentType);
        }


        public async Task<bool> DeleteDiaryEntryByIdAsync(int id, string userId)
        {
            var entry = await _diaryRepository.GetByIdAsync(id, userId);

            if (entry == null)
                return false;

            foreach (var page in entry.Pages)
                await _fileStorage.DeleteFileAsync(page.ImagePath);

            await _diaryRepository.DeleteAsync(entry);
            await _diaryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteDiaryEntriesByDateAsync(string userId, DateOnly date)
        {
            var entries = await GetDiaryEntriesAsync(userId, date);
            return await _diaryRepository.DeleteByDateAsync(entries);
        }
    }
}
