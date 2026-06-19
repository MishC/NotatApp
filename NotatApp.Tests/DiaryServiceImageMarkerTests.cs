using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using NotatApp.Models;
using NotatApp.Repositories.DiaryRepositories;
using NotatApp.Services.DiaryServices;
using Xunit;

namespace NotatApp.Tests
{
    public class DiaryServiceImageMarkerTests
    {
        private const string UserId = "user-123";

        [Fact]
        public async Task CreateDiaryPage_WithBlobImageInHtml_ReplacesItWithBackendMarker()
        {
            // Frontend sends multipart/form-data:
            // pageNumber=2
            // content=<p>Hello</p><img src="blob:http://localhost/image"><p>Bye</p>
            // image=<file>
            var entry = new DiaryEntry
            {
                Id = 5,
                Title = "Friday",
                Date = new DateOnly(2026, 6, 19),
                UserId = UserId
            };
            var image = new Mock<IFormFile>().Object;
            var dto = new CreateDiaryPageDto
            {
                PageNumber = 2,
                Content = """<p>Hello</p><img src="blob:http://localhost/image"><p>Bye</p>""",
                Image = image
            };

            DiaryPage? capturedPage = null;
            var repository = new Mock<IDiaryRepository>();
            repository
                .Setup(r => r.GetByIdAsync(5, UserId))
                .ReturnsAsync(entry);
            repository
                .Setup(r => r.AddPageAsync(It.IsAny<DiaryPage>()))
                .Callback<DiaryPage>(page => capturedPage = page)
                .Returns(Task.CompletedTask);
            repository
                .Setup(r => r.SaveChangesAsync())
                .Callback(() =>
                {
                    if (capturedPage is { Id: 0 })
                        capturedPage.Id = 123;
                })
                .Returns(Task.CompletedTask);

            var fileStorage = new Mock<IFileStorageService>();
            fileStorage
                .Setup(s => s.SaveDiaryImageAsync(image, UserId))
                .ReturnsAsync(new StoredFileResult
                {
                    ImagePath = "data/private-uploads/diary/user-123/image.png",
                    ImageContentType = "image/png",
                    ImageFileName = "image.png"
                });

            var service = new DiaryService(repository.Object, fileStorage.Object);

            var result = await service.CreateDiaryPageAsync(5, dto, UserId);

            Assert.NotNull(result);
            Assert.Equal(123, result!.Id);
            Assert.Equal("""<p>Hello</p><img data-diary-page-image-id="123"><p>Bye</p>""", result.Content);
            Assert.Equal("data/private-uploads/diary/user-123/image.png", result.ImagePath);
            Assert.Equal("image/png", result.ImageContentType);
            Assert.Equal("image.png", result.ImageFileName);
            repository.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task CreateDiaryEntry_WithPlaceholder_ReplacesItWithFirstPageMarker()
        {
            // Frontend sends multipart/form-data:
            // title=Friday
            // date=2026-06-19
            // content=<p>Hello</p>{{diary-page-image}}
            // image=<file>
            var image = new Mock<IFormFile>().Object;
            var dto = new CreateDiaryEntryDto
            {
                Title = "Friday",
                Date = new DateOnly(2026, 6, 19),
                Content = "<p>Hello</p>{{diary-page-image}}",
                Image = image
            };

            DiaryEntry? capturedEntry = null;
            var repository = new Mock<IDiaryRepository>();
            repository
                .Setup(r => r.GetByDateAsync(UserId, new DateOnly(2026, 6, 19)))
                .ReturnsAsync([]);
            repository
                .Setup(r => r.AddAsync(It.IsAny<DiaryEntry>()))
                .Callback<DiaryEntry>(entry => capturedEntry = entry)
                .Returns(Task.CompletedTask);
            repository
                .Setup(r => r.SaveChangesAsync())
                .Callback(() =>
                {
                    if (capturedEntry is { Id: 0 })
                        capturedEntry.Id = 10;

                    var page = capturedEntry?.Pages[0];
                    if (page is { Id: 0 })
                        page.Id = 456;
                })
                .Returns(Task.CompletedTask);

            var fileStorage = new Mock<IFileStorageService>();
            fileStorage
                .Setup(s => s.SaveDiaryImageAsync(image, UserId))
                .ReturnsAsync(new StoredFileResult
                {
                    ImagePath = "data/private-uploads/diary/user-123/first-page.webp",
                    ImageContentType = "image/webp",
                    ImageFileName = "first-page.webp"
                });

            var service = new DiaryService(repository.Object, fileStorage.Object);

            var result = await service.CreateDiaryEntryAsync(UserId, dto);
            var firstPage = result.Pages[0];

            Assert.Equal(10, result.Id);
            Assert.Equal(456, firstPage.Id);
            Assert.Equal("""<p>Hello</p><img data-diary-page-image-id="456">""", firstPage.Content);
            Assert.Equal("data/private-uploads/diary/user-123/first-page.webp", firstPage.ImagePath);
            repository.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task UpdateDiaryPage_RemoveImage_RemovesBackendMarkerFromHtml()
        {
            var page = new DiaryPage
            {
                Id = 31,
                DiaryEntryId = 5,
                PageNumber = 2,
                Content = """<p>Hello</p><img data-diary-page-image-id="31"><p>Bye</p>""",
                ImagePath = "data/private-uploads/diary/user-123/image.png",
                ImageContentType = "image/png",
                ImageFileName = "image.png"
            };

            var repository = new Mock<IDiaryRepository>();
            repository
                .Setup(r => r.GetPageByIdAsync(31, UserId))
                .ReturnsAsync(page);
            repository
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            var fileStorage = new Mock<IFileStorageService>();
            fileStorage
                .Setup(s => s.DeleteFileAsync(page.ImagePath))
                .Returns(Task.CompletedTask);

            var service = new DiaryService(repository.Object, fileStorage.Object);

            var updated = await service.UpdateDiaryPageAsync(
                31,
                new UpdateDiaryPageDto { RemoveImage = true },
                UserId);

            Assert.True(updated);
            Assert.Equal("<p>Hello</p><p>Bye</p>", page.Content);
            Assert.Null(page.ImagePath);
            Assert.Null(page.ImageContentType);
            Assert.Null(page.ImageFileName);
            fileStorage.Verify(s => s.DeleteFileAsync("data/private-uploads/diary/user-123/image.png"), Times.Once);
        }

        [Fact]
        public async Task CreateDiaryEntry_WhenDateAlreadyExists_ThrowsBeforeInsert()
        {
            var existing = new DiaryEntry
            {
                Id = 10,
                Title = "Existing",
                Date = new DateOnly(2026, 6, 19),
                UserId = UserId
            };

            var repository = new Mock<IDiaryRepository>();
            repository
                .Setup(r => r.GetByDateAsync(UserId, new DateOnly(2026, 6, 19)))
                .ReturnsAsync([existing]);

            var service = new DiaryService(repository.Object, Mock.Of<IFileStorageService>());

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateDiaryEntryAsync(
                    UserId,
                    new CreateDiaryEntryDto
                    {
                        Title = "Duplicate",
                        Date = new DateOnly(2026, 6, 19),
                        Content = "Text"
                    }));

            Assert.Contains("already exists", ex.Message);
            repository.Verify(r => r.AddAsync(It.IsAny<DiaryEntry>()), Times.Never);
            repository.Verify(r => r.SaveChangesAsync(), Times.Never);
        }
    }
}
