using Bookmark.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookmark.API.Controllers
{
    [ApiController]
    [Route("api/bookmarks")]
    public class BookmarkController : ControllerBase
    {
        private readonly IBookmarkService _bookmarkService;

        public BookmarkController(IBookmarkService bookmarkService)
        {
            _bookmarkService = bookmarkService;
        }

        // POST /api/bookmarks/toggle/{postId}
        [Authorize]
        [HttpPost("toggle/{postId}")]
        public async Task<IActionResult> Toggle(int postId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int userId = int.Parse(userIdClaim.Value);

                bool bookmarked = await _bookmarkService.ToggleBookmark(userId, postId);
                return Ok(new { postId, bookmarked, message = bookmarked ? "Post bookmarked." : "Bookmark removed." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/bookmarks
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUserBookmarks()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int userId = int.Parse(userIdClaim.Value);

                var bookmarks = await _bookmarkService.GetUserBookmarks(userId);
                return Ok(bookmarks);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/bookmarks/check/{postId}
        [Authorize]
        [HttpGet("check/{postId}")]
        public async Task<IActionResult> IsBookmarked(int postId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int userId = int.Parse(userIdClaim.Value);

            bool bookmarked = await _bookmarkService.IsBookmarked(userId, postId);
            return Ok(new { postId, bookmarked });
        }
    }
}
