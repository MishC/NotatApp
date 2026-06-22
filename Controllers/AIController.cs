using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Extensions;
using NotatApp.Repositories.DiaryRepositories;
using NotatApp.Services.DiaryServices;

namespace NotatApp.Controllers
{
    [ApiController]
    [Route("api/AI")]
    [Authorize]
    public class AIController : ControllerBase
    {
        private readonly IDiaryRepository _diaryRepository;
        private readonly IRecommendations _recommendations;

        public AIController(
            IDiaryRepository diaryRepository,
            IRecommendations recommendations)
        {
            _diaryRepository = diaryRepository;
            _recommendations = recommendations;
        }

        [HttpPost("song")]
        public async Task<IActionResult> RecommendSong([FromBody] AiSongRequest request)
        {
            var userId = User.GetUserId();

            if (request.DiaryEntryId <= 0)
                return BadRequest(new { message = "DiaryEntryId is required." });

            var entry = await _diaryRepository.GetByIdAsync(request.DiaryEntryId, userId);
            if (entry is null)
                return NotFound();

            try
            {
                var songs = await _recommendations.GetAnswerOnPrompt(userId, entry, request.Style);

                return Ok(songs.Select(song => new
                {
                    song.Title,
                    song.Artist,
                    song.Link
                }));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class AiSongRequest
    {
        public int DiaryEntryId { get; set; }
        public string? Style { get; set; }
    }
}
