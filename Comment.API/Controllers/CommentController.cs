using Comment.API.DTOs;
using Comment.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Comment.API.Controllers
{
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        // POST /api/comments
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentDto dto)
        {
            try
            {
                var comment = await _commentService.AddComment(dto);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/comments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var comment = await _commentService.GetCommentById(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        // GET /api/comments/post/{postId}
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetByPost(int postId)
        {
            var comments = await _commentService.GetCommentsByPost(postId);
            return Ok(comments);
        }

        // GET /api/comments/post/{postId}/topLevel
        [HttpGet("post/{postId}/topLevel")]
        public async Task<IActionResult> GetTopLevel(int postId)
        {
            var comments = await _commentService.GetTopLevelComments(postId);
            return Ok(comments);
        }

        // GET /api/comments/replies/{commentId}
        [HttpGet("replies/{commentId}")]
        public async Task<IActionResult> GetReplies(int commentId)
        {
            var replies = await _commentService.GetReplies(commentId);
            return Ok(replies);
        }

        // GET /api/comments/user/{userId}
        [Authorize]
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var comments = await _commentService.GetCommentsByUser(userId);
            return Ok(comments);
        }

        // GET /api/comments/count/{postId}
        [HttpGet("count/{postId}")]
        public async Task<IActionResult> GetCount(int postId)
        {
            var count = await _commentService.GetCommentCount(postId);
            return Ok(new { postId, commentCount = count });
        }

        // PUT /api/comments/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> EditComment(int id, [FromBody] EditCommentDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int requestingUserId = int.Parse(userIdClaim.Value);

                var updated = await _commentService.EditComment(
                    id, requestingUserId, dto.Content);
                return Ok(updated);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/comments/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int requestingUserId = int.Parse(userIdClaim.Value);

                await _commentService.DeleteComment(id, requestingUserId);
                return Ok(new { message = "Comment deleted successfully." });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/comments/{id}/counts ← called by Like API
        [HttpPut("{id}/counts")]
        public async Task<IActionResult> UpdateCount(int id, [FromBody] IncrementCountDto dto)
        {
            try
            {
                if (dto.Field == "LikeCount")
                    await _commentService.IncrementLikeCount(id, dto.Delta);
                else
                    return BadRequest(new { message = "Invalid field name." });

                return Ok(new { message = "Count updated." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}