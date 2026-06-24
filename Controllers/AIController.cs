using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotatApp.Extensions;
using NotatApp.Repositories.DiaryRepositories;
using NotatApp.Services.DiaryServices;
using NotatApp.Models;

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
                var songs = await _recommendations.GetAnswerOnPrompt(
                    userId,
                    entry,
                    request.Style,
                    request.Country);

                return Ok(songs.Select(song => new
                {
                    song.Title,
                    song.Artist,
                    song.Link,
                    song.Style,
                    song.Country
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

        [HttpPost("frame")]
        public async Task<IActionResult> GenerateFrame([FromBody] AiFrameRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Description))
                return BadRequest(new { message = "Description is required." });

            try
            {
                var css = await _recommendations.GetFrame(request.Description);
                return Ok(new { css });
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

}
