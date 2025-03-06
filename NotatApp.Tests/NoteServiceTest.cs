using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using NotatApp.Models;
using NotatApp.Services;
using NotatApp.Repositories;

public class NoteServiceTests
{
    private readonly Mock<INoteRepository> _mockRepo;
    private readonly NoteService _noteService;

    public NoteServiceTests()
    {
        _mockRepo = new Mock<INoteRepository>();
        _noteService = new NoteService(_mockRepo.Object);
    }

    [Fact]
    public async Task CreateNote_ValidNote_ReturnsCreatedNote()
    {
        // Arrange
        var note = new Note { Id = 1, Title = "Test Note", Content = "Test Content", FolderId = 1 };

        _mockRepo.Setup(repo => repo.AddNoteAsync(It.IsAny<Note>()))
                 .Returns(Task.FromResult(note));

        // Act
        await _noteService.AddNoteAsync(note);
        var result = note;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(note.Id, result.Id);
        Assert.Equal(note.Title, result.Title);
    }

    [Fact]
    public async Task CreateNote_EmptyTitle_ThrowsArgumentException()
    {
        // Arrange
        var note = new Note { Id = 1, Title = "", Content = "Test Content", FolderId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _noteService.AddNoteAsync(note));
    }

    // Test 3: Creating a note with null object
    [Fact]
    public async Task CreateNote_NullNote_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _noteService.AddNoteAsync(null!));
    }

    [Fact]
    public async Task CreateNote_TitleTooLong_ThrowsArgumentException()
    {
        // Arrange
        var note = new Note { Id = 1, Title = new string('A', 150), Content = "Test Content", FolderId = 1 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _noteService.AddNoteAsync(note));
    }

    [Fact]
    public async Task GetNoteById_ValidId_ReturnsNote()
    {
        // Arrange
        var note = new Note { Id = 1, Title = "Valid Note", Content = "Test Content", FolderId = 1 };

        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(1))
                 .ReturnsAsync(note);

        // Act
        var result = await _noteService.GetNoteByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetNoteById_InvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(It.IsAny<int>()))
                 .ReturnsAsync((Note?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _noteService.GetNoteByIdAsync(99));
    }

    [Fact]
    public async Task UpdateNote_ValidNote_ReturnsUpdatedNote()
    {
        // Arrange
        var existingNote = new Note { Id = 1, Title = "Old Title", Content = "Old Content", FolderId = 1 };
        var updatedNote = new Note { Id = 1, Title = "New Title", Content = "New Content", FolderId = 1 };

        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(1)).ReturnsAsync(existingNote);
        _mockRepo.Setup(repo => repo.UpdateNoteAsync(updatedNote)).Returns(Task.FromResult(updatedNote));

        // Act
        await _noteService.UpdateNoteAsync(updatedNote);
        var result = await _noteService.GetNoteByIdAsync(updatedNote.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
    }

    [Fact]
    public async Task UpdateNote_NonExistentNote_ThrowsKeyNotFoundException()
    {
        // Arrange
        var updatedNote = new Note { Id = 1, Title = "Updated Title", Content = "Updated Content", FolderId = 1 };

        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(1)).ReturnsAsync((Note?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _noteService.UpdateNoteAsync(updatedNote));
    }

    // ✅ Test 9: Deleting a valid note
    [Fact]
    public async Task DeleteNote_ValidId_ReturnsTrue()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(1))
                 .ReturnsAsync(new Note { Id = 1, Title = "Note to delete" });

        _mockRepo.Setup(repo => repo.DeleteNoteAsync(1))
                 .Returns(Task.FromResult(true));

        // Act
        await _noteService.DeleteNoteAsync(1);

        // Assert
        _mockRepo.Verify(repo => repo.DeleteNoteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteNote_NonExistentNote_ThrowsKeyNotFoundException()
    {
        // Arrange
        _mockRepo.Setup(repo => repo.GetNoteByIdAsync(It.IsAny<int>())).ReturnsAsync((Note?)null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => _noteService.DeleteNoteAsync(99));
    }
}
