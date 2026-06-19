using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NotatApp.Controllers;
using NotatApp.Models;
using NotatApp.Services;
using NotatApp.Services.DiaryServices;
using Xunit;

namespace NotatApp.Tests
{
    public class ApiResponseContractTests
    {
        private const string UserId = "user-123";
        private static readonly JsonSerializerOptions WebJsonOptions = new(JsonSerializerDefaults.Web);

        [Fact]
        public async Task Notes_GetAll_ReturnsExpectedResponseValues()
        {
            // Frontend request:
            // GET /api/notes
            // Authorization: Bearer <jwt>
            var note = new Note
            {
                Id = 7,
                Title = "Math homework",
                Content = "Finish chapter 3",
                FolderId = 2,
                IsDone = false,
                ScheduledAt = new DateOnly(2026, 6, 20),
                OrderIndex = 4,
                UserId = UserId
            };

            var service = new Mock<INoteService>();
            service
                .Setup(s => s.GetAllNotesAsync(UserId))
                .ReturnsAsync(new List<Note> { note });

            var controller = CreateNoteController(service.Object);

            var action = await controller.GetAllNotes();

            var ok = Assert.IsType<OkObjectResult>(action);
            using var json = ToJsonDocument(ok.Value);
            var first = json.RootElement[0];

            Assert.Equal(7, first.GetProperty("id").GetInt32());
            Assert.Equal("Math homework", first.GetProperty("title").GetString());
            Assert.Equal("Finish chapter 3", first.GetProperty("content").GetString());
            Assert.Equal(2, first.GetProperty("folderId").GetInt32());
            Assert.False(first.GetProperty("isDone").GetBoolean());
            Assert.Equal("2026-06-20", first.GetProperty("scheduledAt").GetString());
            Assert.Equal(4, first.GetProperty("orderIndex").GetInt32());
            Assert.Equal(UserId, first.GetProperty("userId").GetString());
        }

        [Fact]
        public async Task Notes_Create_ExpectsJsonBodyAndReturnsCreatedNote()
        {
            // Frontend request:
            // POST /api/notes
            // Content-Type: application/json
            // { "title": "Shopping", "content": "Milk", "folderId": 3, "scheduledAt": "2026-06-21" }
            var dto = new CreateNoteDto
            {
                Title = "Shopping",
                Content = "Milk",
                FolderId = 3,
                ScheduledAt = new DateOnly(2026, 6, 21)
            };

            var created = new Note
            {
                Id = 11,
                Title = dto.Title,
                Content = dto.Content,
                FolderId = dto.FolderId,
                ScheduledAt = dto.ScheduledAt,
                IsDone = false,
                OrderIndex = 0,
                UserId = UserId
            };

            var service = new Mock<INoteService>();
            service
                .Setup(s => s.CreateNoteAsync(dto, UserId))
                .ReturnsAsync(created);

            var controller = CreateNoteController(service.Object);

            var action = await controller.CreateNote(dto);

            var createdAt = Assert.IsType<CreatedAtActionResult>(action);
            Assert.Equal(nameof(NoteController.GetNoteById), createdAt.ActionName);
            Assert.Equal(11, createdAt.RouteValues!["id"]);

            using var json = ToJsonDocument(createdAt.Value);
            Assert.Equal(11, json.RootElement.GetProperty("id").GetInt32());
            Assert.Equal("Shopping", json.RootElement.GetProperty("title").GetString());
            Assert.Equal("Milk", json.RootElement.GetProperty("content").GetString());
            Assert.Equal(3, json.RootElement.GetProperty("folderId").GetInt32());
            Assert.Equal("2026-06-21", json.RootElement.GetProperty("scheduledAt").GetString());
            Assert.False(json.RootElement.GetProperty("isDone").GetBoolean());
            Assert.Equal(UserId, json.RootElement.GetProperty("userId").GetString());
        }

        [Fact]
        public async Task Diary_GetByDate_ReturnsEntryWithPages()
        {
            // Frontend request:
            // GET /api/diary?date=2026-06-19
            // Authorization: Bearer <jwt>
            var createdAt = new DateTime(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc);
            var entry = new DiaryEntry
            {
                Id = 5,
                Title = "Friday",
                Date = new DateOnly(2026, 6, 19),
                CreatedAt = createdAt,
                UserId = UserId,
                Pages =
                [
                    new DiaryPage
                    {
                        Id = 30,
                        PageNumber = 1,
                        Content = "First page text",
                        ImagePath = "data/private-uploads/diary/user-123/image.png",
                        ImageFileName = "image.png",
                        ImageContentType = "image/png",
                        CreatedAt = createdAt,
                        ImageUploadedAt = createdAt
                    },
                    new DiaryPage
                    {
                        Id = 31,
                        PageNumber = 2,
                        Content = "Second page text",
                        CreatedAt = createdAt
                    }
                ]
            };

            var service = new Mock<IDiaryService>();
            service
                .Setup(s => s.GetDiaryEntriesAsync(UserId, new DateOnly(2026, 6, 19)))
                .ReturnsAsync(new List<DiaryEntry> { entry });

            var controller = CreateDiaryController(service.Object);

            var action = await controller.GetDiaryEntries(new DateOnly(2026, 6, 19));

            var ok = Assert.IsType<OkObjectResult>(action);
            using var json = ToJsonDocument(ok.Value);
            var entryJson = json.RootElement[0];
            var pages = entryJson.GetProperty("pages");

            Assert.Equal(5, entryJson.GetProperty("id").GetInt32());
            Assert.Equal("Friday", entryJson.GetProperty("title").GetString());
            Assert.Equal("2026-06-19", entryJson.GetProperty("date").GetString());
            Assert.Equal(2, pages.GetArrayLength());

            Assert.Equal(30, pages[0].GetProperty("id").GetInt32());
            Assert.Equal(1, pages[0].GetProperty("pageNumber").GetInt32());
            Assert.Equal("First page text", pages[0].GetProperty("content").GetString());
            Assert.True(pages[0].GetProperty("hasImage").GetBoolean());
            Assert.Equal("image.png", pages[0].GetProperty("imageFileName").GetString());

            Assert.Equal(31, pages[1].GetProperty("id").GetInt32());
            Assert.Equal(2, pages[1].GetProperty("pageNumber").GetInt32());
            Assert.Equal("Second page text", pages[1].GetProperty("content").GetString());
            Assert.False(pages[1].GetProperty("hasImage").GetBoolean());
        }

        [Fact]
        public async Task Diary_CreateEntry_ExpectsMultipartFormAndReturnsEntryWithPage()
        {
            // Frontend request:
            // POST /api/diary
            // Content-Type: multipart/form-data
            // fields: title=Friday, date=2026-06-19, content=First page text, image=<optional file>
            var dto = new CreateDiaryEntryDto
            {
                Title = "Friday",
                Date = new DateOnly(2026, 6, 19),
                Content = "First page text",
                Image = null
            };

            var entry = new DiaryEntry
            {
                Id = 5,
                Title = dto.Title,
                Date = dto.Date,
                CreatedAt = new DateTime(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc),
                UserId = UserId,
                Pages =
                [
                    new DiaryPage
                    {
                        Id = 30,
                        PageNumber = 1,
                        Content = dto.Content,
                        CreatedAt = new DateTime(2026, 6, 19, 10, 0, 0, DateTimeKind.Utc)
                    }
                ]
            };

            var service = new Mock<IDiaryService>();
            service
                .Setup(s => s.CreateDiaryEntryAsync(UserId, dto))
                .ReturnsAsync(entry);

            var controller = CreateDiaryController(service.Object);

            var action = await controller.CreateDiaryEntry(dto);

            var ok = Assert.IsType<OkObjectResult>(action);
            using var json = ToJsonDocument(ok.Value);
            var pages = json.RootElement.GetProperty("pages");

            Assert.Equal(5, json.RootElement.GetProperty("id").GetInt32());
            Assert.Equal("Friday", json.RootElement.GetProperty("title").GetString());
            Assert.Equal("2026-06-19", json.RootElement.GetProperty("date").GetString());
            Assert.Single(pages.EnumerateArray());
            Assert.Equal(30, pages[0].GetProperty("id").GetInt32());
            Assert.Equal(1, pages[0].GetProperty("pageNumber").GetInt32());
            Assert.Equal("First page text", pages[0].GetProperty("content").GetString());
            Assert.False(pages[0].GetProperty("hasImage").GetBoolean());
        }

        [Fact]
        public async Task Diary_CreatePage_ExpectsMultipartFormAndReturnsPage()
        {
            // Frontend request:
            // POST /api/diary/5/pages
            // Content-Type: multipart/form-data
            // fields: pageNumber=2, content=Second page text, image=<optional file>
            var dto = new CreateDiaryPageDto
            {
                PageNumber = 2,
                Content = "Second page text",
                Image = null
            };

            var page = new DiaryPage
            {
                Id = 31,
                DiaryEntryId = 5,
                PageNumber = dto.PageNumber,
                Content = dto.Content,
                CreatedAt = new DateTime(2026, 6, 19, 10, 30, 0, DateTimeKind.Utc)
            };

            var service = new Mock<IDiaryService>();
            service
                .Setup(s => s.CreateDiaryPageAsync(5, dto, UserId))
                .ReturnsAsync(page);

            var controller = CreateDiaryController(service.Object);

            var action = await controller.CreateDiaryPage(5, dto);

            var ok = Assert.IsType<OkObjectResult>(action);
            using var json = ToJsonDocument(ok.Value);

            Assert.Equal(31, json.RootElement.GetProperty("id").GetInt32());
            Assert.Equal(2, json.RootElement.GetProperty("pageNumber").GetInt32());
            Assert.Equal("Second page text", json.RootElement.GetProperty("content").GetString());
            Assert.False(json.RootElement.GetProperty("hasImage").GetBoolean());
        }

        [Fact]
        public async Task Diary_DeletePage_WhenServiceDeletes_ReturnsNoContent()
        {
            // Frontend request:
            // DELETE /api/diary/pages/31
            var service = new Mock<IDiaryService>();
            service
                .Setup(s => s.DeleteDiaryPageAsync(31, UserId))
                .ReturnsAsync(true);

            var controller = CreateDiaryController(service.Object);

            var action = await controller.DeleteDiaryPage(31);

            Assert.IsType<NoContentResult>(action);
        }

        private static NoteController CreateNoteController(INoteService service)
        {
            var controller = new NoteController(service);
            SetAuthenticatedUser(controller);
            return controller;
        }

        private static DiaryController CreateDiaryController(IDiaryService service)
        {
            var controller = new DiaryController(service);
            SetAuthenticatedUser(controller);
            return controller;
        }

        private static void SetAuthenticatedUser(ControllerBase controller)
        {
            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, UserId),
                    new Claim(JwtRegisteredClaimNames.Sub, UserId)
                ],
                "TestAuth");

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(identity)
                }
            };
        }

        private static JsonDocument ToJsonDocument(object? value)
        {
            var json = JsonSerializer.Serialize(value, WebJsonOptions);
            return JsonDocument.Parse(json);
        }
    }
}
