using NotatApp.Models;
using NotatApp.Repositories.DiaryRepositories;
using System.Text.RegularExpressions;

namespace NotatApp.Services.DiaryServices
{
    public class DiaryService(
        IDiaryRepository diaryRepository,
        IFileStorageService fileStorage) : IDiaryService
    {
        private readonly IDiaryRepository _diaryRepository = diaryRepository;
        private readonly IFileStorageService _fileStorage = fileStorage;
        private const string DiaryPageImagePlaceholder = "{{diary-page-image}}";
        private static readonly Regex LocalImageTagRegex = new(
            """<img\b(?=[^>]*\bsrc\s*=\s*["'](?:data:|blob:))[^>]*>""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DiaryPageImageMarkerRegex = new(
            """<img\b(?=[^>]*\bdata-diary-page-image-id\s*=\s*["']\d+["'])[^>]*>""",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var existingEntries = await _diaryRepository.GetByDateAsync(userId, dto.Date);
            if (existingEntries.Count > 0)
                throw new InvalidOperationException("Diary entry already exists for this date. Add or update diary pages on the existing entry.");

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

            if (dto.Image != null)
            {
                page.Content = ApplyDiaryPageImageMarker(page.Content, page.Id);
                await _diaryRepository.SaveChangesAsync();
            }

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

        public async Task<DiaryPage?> CreateDiaryPageAsync(
            int entryId,
            CreateDiaryPageDto dto,
            string userId
        )
        {
            var entry = await _diaryRepository.GetByIdAsync(entryId, userId);

            if (entry == null)
                return null;

            if (entry.Pages.Any(p => p.PageNumber == dto.PageNumber))
                throw new InvalidOperationException("Diary page with this page number already exists for this entry.");

            var page = new DiaryPage
            {
                DiaryEntryId = entry.Id,
                PageNumber = dto.PageNumber,
                Content = dto.Content
            };

            if (dto.Image != null)
            {
                var stored = await _fileStorage.SaveDiaryImageAsync(dto.Image, userId);

                page.ImagePath = stored.ImagePath;
                page.ImageContentType = stored.ImageContentType;
                page.ImageFileName = stored.ImageFileName;
                page.ImageUploadedAt = DateTime.UtcNow;
            }

            await _diaryRepository.AddPageAsync(page);
            await _diaryRepository.SaveChangesAsync();

            if (dto.Image != null)
            {
                page.Content = ApplyDiaryPageImageMarker(page.Content, page.Id);
                await _diaryRepository.SaveChangesAsync();
            }

            return page;
        }

        public async Task<bool> UpdateDiaryPageAsync(
            int pageId,
            UpdateDiaryPageDto dto,
            string userId
        )
        {
            var page = await _diaryRepository.GetPageByIdAsync(pageId, userId);

            if (page == null)
                return false;

            if (dto.Content != null)
                page.Content = dto.Content;

            if (dto.RemoveImage)
            {
                await _fileStorage.DeleteFileAsync(page.ImagePath);

                page.ImagePath = null;
                page.ImageContentType = null;
                page.ImageFileName = null;
                page.ImageUploadedAt = null;
                page.Content = RemoveDiaryPageImageMarker(page.Content);
            }

            if (dto.Image != null)
            {
                await _fileStorage.DeleteFileAsync(page.ImagePath);

                var stored = await _fileStorage.SaveDiaryImageAsync(dto.Image, userId);

                page.ImagePath = stored.ImagePath;
                page.ImageContentType = stored.ImageContentType;
                page.ImageFileName = stored.ImageFileName;
                page.ImageUploadedAt = DateTime.UtcNow;
                page.Content = ApplyDiaryPageImageMarker(page.Content, page.Id);
            }

            page.UpdatedAt = DateTime.UtcNow;
            await _diaryRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteDiaryPageAsync(int pageId, string userId)
        {
            var page = await _diaryRepository.GetPageByIdAsync(pageId, userId);

            if (page == null)
                return false;

            await _fileStorage.DeleteFileAsync(page.ImagePath);
            await _diaryRepository.DeletePageAsync(page);
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

        public async Task<(string absolutePath, string contentType)?> GetDiaryPageImageAsync(
            int pageId,
            string userId
        )
        {
            var page = await _diaryRepository.GetPageByIdAsync(pageId, userId);

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

            foreach (var entry in entries)
            {
                foreach (var page in entry.Pages)
                    await _fileStorage.DeleteFileAsync(page.ImagePath);
            }

            await _diaryRepository.DeleteByDateAsync(entries);
            await _diaryRepository.SaveChangesAsync();

            return entries.Count > 0;
        }

        private static string? ApplyDiaryPageImageMarker(string? content, int pageId)
        {
            var marker = $"""<img data-diary-page-image-id="{pageId}">""";

            if (string.IsNullOrWhiteSpace(content))
                return marker;

            if (content.Contains(DiaryPageImagePlaceholder, StringComparison.Ordinal))
                return content.Replace(DiaryPageImagePlaceholder, marker, StringComparison.Ordinal);

            if (LocalImageTagRegex.IsMatch(content))
                return LocalImageTagRegex.Replace(content, marker, 1);

            if (DiaryPageImageMarkerRegex.IsMatch(content))
                return DiaryPageImageMarkerRegex.Replace(content, marker, 1);

            return $"{content}{marker}";
        }

        private static string? RemoveDiaryPageImageMarker(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return content;

            return DiaryPageImageMarkerRegex.Replace(content, string.Empty, 1);
        }
    }
}
