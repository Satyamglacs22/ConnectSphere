using Feed.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Feed.API.Controllers
{
    [ApiController]
    [Route("api/feed")]
    public class FeedController : ControllerBase
    {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService)
        {
            _feedService = feedService;
        }

        // GET /api/feed/{userId}?page=1&pageSize=20
        [Authorize]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetFeed(
            int userId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var feed = await _feedService.GetFeed(
                    userId, page, pageSize);
                return Ok(feed);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/feed/{userId}/unseen
        [Authorize]
        [HttpGet("{userId}/unseen")]
        public async Task<IActionResult> GetUnseen(int userId)
        {
            try
            {
                var unseen = await _feedService.GetUnseenFeed(userId);
                return Ok(unseen);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/feed/{userId}/unseenCount
        [Authorize]
        [HttpGet("{userId}/unseenCount")]
        public async Task<IActionResult> GetUnseenCount(int userId)
        {
            try
            {
                var count = await _feedService.GetUnseenCount(userId);
                return Ok(new { userId, unseenCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/feed/{userId}/markSeen
        [Authorize]
        [HttpPut("{userId}/markSeen")]
        public async Task<IActionResult> MarkSeen(int userId)
        {
            try
            {
                await _feedService.MarkFeedAsSeen(userId);
                return Ok(new { message = "Feed marked as seen." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}