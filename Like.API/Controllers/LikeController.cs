using Like.API.DTOs;
using Like.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Like.API.Controllers
{
    [ApiController]
    [Route("api/likes")]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService)
        {
            _likeService = likeService;
        }

        // POST /api/likes/toggle
        [Authorize]
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeDto dto)
        {
            try
            {
                var (liked, likeCount) = await _likeService.ToggleLike(
                    dto.UserId, dto.TargetId, dto.TargetType);

                return Ok(new ToggleLikeResponseDto
                {
                    Liked = liked,
                    LikeCount = likeCount,
                    Message = liked ? "Liked successfully." : "Unliked successfully."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/likes/target/{targetId}/{type}
        [HttpGet("target/{targetId}/{type}")]
        public async Task<IActionResult> GetLikesByTarget(int targetId, string type)
        {
            var likes = await _likeService.GetLikesByTarget(targetId, type);
            return Ok(likes);
        }

        // GET /api/likes/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetLikesByUser(int userId)
        {
            var likes = await _likeService.GetLikesByUser(userId);
            return Ok(likes);
        }

        // GET /api/likes/count/{targetId}/{type}
        [HttpGet("count/{targetId}/{type}")]
        public async Task<IActionResult> GetLikeCount(int targetId, string type)
        {
            var count = await _likeService.GetLikeCount(targetId, type);
            return Ok(new { targetId, type, likeCount = count });
        }

        // GET /api/likes/hasLiked/{userId}/{targetId}/{type}
        [Authorize]
        [HttpGet("hasLiked/{userId}/{targetId}/{type}")]
        public async Task<IActionResult> HasLiked(int userId, int targetId, string type)
        {
            var hasLiked = await _likeService.HasUserLiked(userId, targetId, type);
            return Ok(new { userId, targetId, type, hasLiked });
        }

        // GET /api/likes/target/{targetId}/{type}/likers
        [Authorize]
        [HttpGet("target/{targetId}/{type}/likers")]
        public async Task<IActionResult> GetLikers(int targetId, string type)
        {
            var likerIds = await _likeService.GetLikersForTarget(targetId, type);
            return Ok(new { targetId, type, likerIds });
        }

        // GET /api/likes/user/{userId}/posts
        [Authorize]
        [HttpGet("user/{userId}/posts")]
        public async Task<IActionResult> GetLikedPosts(int userId)
        {
            var postIds = await _likeService.GetLikedPostsByUser(userId);
            return Ok(new { userId, likedPostIds = postIds });
        }
    }
}