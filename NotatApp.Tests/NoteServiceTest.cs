using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NotatApp.Models;
using NotatApp.Repositories;
using NotatApp.Services;
using Xunit;

namespace NotatApp.Tests
{
    public class NoteServiceTests
    {
        private readonly Mock<INoteRepository> _mockRepo;
        private readonly NoteService _noteService;
        private const string UserId = "user-123";

        public NoteServiceTests()
        {
            _mockRepo = new Mock<INoteRepository>();
            _noteService = new NoteService(_mockRepo.Object);
        }

        // ---------- CREATE ----------

        [Fact]
        public async Task CreateNote_ValidInput_CreatesNoteWithUserId_AndCallsRepository()
        {
            // Arrange
            var dto = new CreateNoteDto
            {
                Title = "Test Note",
                Content = "Test Content",
                FolderId = 1
            };

            _mockRepo
                .Setup(r => r.GetNextOrderIndexAsync(UserId))
                .ReturnsAsync(0);

            _mockRepo
                .Setup(r => r.AddNoteAsync(It.IsAny<Note>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _noteService.CreateNoteAsync(dto, UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal(dto.Content, result.Content);
            Assert.Equal(dto.FolderId, result.FolderId);
            Assert.Equal(UserId, result.UserId);
            Assert.Equal(0, result.OrderIndex);

            _mockRepo.Verify(
                r => r.AddNoteAsync(It.Is<Note>(n =>
                    n.Title == dto.Title &&
                    n.Content == dto.Content &&
                    n.FolderId == dto.FolderId &&
                    n.UserId == UserId &&
                    n.OrderIndex == 0
                )),
                Times.Once);
        }

        [Fact]
        public async Task CreateNote_EmptyTitle_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateNoteDto
            {
                Title = "",
                Content = "Test Content",
                FolderId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _noteService.CreateNoteAsync(dto, UserId));
        }

        [Fact]
        public async Task CreateNote_NullDto_ThrowsArgumentNullException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _noteService.CreateNoteAsync(null!, UserId));
        }

        [Fact]
        public async Task CreateNote_TitleTooLong_ThrowsArgumentException()
        {
            // Arrange
            var dto = new CreateNoteDto
            {
                Title = new string('A', 150), // > 100
                Content = "Test Content",
                FolderId = 1
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => _noteService.CreateNoteAsync(dto, UserId));
        }

        // ---------- GET BY ID ----------

        [Fact]
        public async Task GetNoteById_ValidIdAndUser_ReturnsNote()
        {
            // Arrange
            var note = new Note
            {
                Id = 1,
                Title = "Valid Note",
                Content = "Test Content",
                FolderId = 1,
                UserId = UserId
            };

            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(1, UserId))
                .ReturnsAsync(note);

            // Act
            var result = await _noteService.GetNoteByIdAsync(1, UserId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal(UserId, result.UserId);
        }

        [Fact]
        public async Task GetNoteById_NotFound_ReturnsNull()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(It.IsAny<int>(), UserId))
                .ReturnsAsync((Note?)null);

            // Act
            var result = await _noteService.GetNoteByIdAsync(99, UserId);

            // Assert
            Assert.Null(result);
        }

        // ---------- UPDATE ----------

        [Fact]
        public async Task UpdateNote_ValidNote_UpdatesAndReturnsTrue()
        {
            // Arrange
            var existing = new Note
            {
                Id = 1,
                Title = "Old Title",
                Content = "Old Content",
                FolderId = 1,
                UserId = UserId,
                IsDone = false
            };

            var dto = new UpdateNoteDto
            {
                Title = "New Title",
                Content = "New Content",
                FolderId = 2,
                IsDone = true
            };

            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(1, UserId))
                .ReturnsAsync(existing);

            _mockRepo
                .Setup(r => r.UpdateNoteAsync(It.IsAny<Note>()))
                .Returns(Task.CompletedTask);

            // Act
            var updated = await _noteService.UpdateNoteAsync(1, dto, UserId);

            // Assert
            Assert.True(updated);

            _mockRepo.Verify(
                r => r.UpdateNoteAsync(It.Is<Note>(n =>
                    n.Id == 1 &&
                    n.Title == dto.Title &&
                    n.Content == dto.Content &&
                    n.FolderId == dto.FolderId &&
                    n.IsDone  == dto.IsDone &&
                    n.UserId == UserId
                )),
                Times.Once);
        }

        [Fact]
        public async Task UpdateNote_NonExistingNote_ReturnsFalse()
        {
            // Arrange
            var dto = new UpdateNoteDto
            {
                Title = "Updated Title",
                Content = "Updated Content",
                FolderId = 1,
                IsDone = false
            };

            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(It.IsAny<int>(), UserId))
                .ReturnsAsync((Note?)null);

            // Act
            var updated = await _noteService.UpdateNoteAsync(1, dto, UserId);

            // Assert
            Assert.False(updated);
            _mockRepo.Verify(r => r.UpdateNoteAsync(It.IsAny<Note>()), Times.Never);
        }

        // ---------- DELETE ----------

        [Fact]
        public async Task DeleteNote_ExistingNote_ReturnsTrueAndCallsRepository()
        {
            // Arrange
            var note = new Note
            {
                Id = 1,
                Title = "Note to delete",
                UserId = UserId
            };

            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(1, UserId))
                .ReturnsAsync(note);

            _mockRepo
                .Setup(r => r.DeleteNoteAsync(note))
                .Returns(Task.CompletedTask);

            // Act
            var deleted = await _noteService.DeleteNoteAsync(1, UserId);

            // Assert
            Assert.True(deleted);
            _mockRepo.Verify(r => r.DeleteNoteAsync(It.Is<Note>(n => n.Id == 1 && n.UserId == UserId)), Times.Once);
        }

        [Fact]
        public async Task DeleteNote_NonExistingNote_ReturnsFalse()
        {
            // Arrange
            _mockRepo
                .Setup(r => r.GetNoteByIdAsync(It.IsAny<int>(), UserId))
                .ReturnsAsync((Note?)null);

            // Act
            var deleted = await _noteService.DeleteNoteAsync(99, UserId);

            // Assert
            Assert.False(deleted);
            _mockRepo.Verify(r => r.DeleteNoteAsync(It.IsAny<Note>()), Times.Never);
        }
    }

    
}
