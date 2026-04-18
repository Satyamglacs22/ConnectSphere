using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Post.API.DTOs;
using Post.API.Entities;
using Post.API.Services.Interfaces;

namespace Post.API.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

      

        // POST /api/posts
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto dto)
        {
            try
            {
                var post = new PostEntity
                {
                    UserId = dto.UserId,
                    Content = dto.Content,
                    MediaUrl = dto.MediaUrl,
                    MediaType = dto.MediaType,
                    Visibility = dto.Visibility,
                    Hashtags = dto.Hashtags
                };

                var created = await _postService.CreatePost(post);
                return Ok(created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var post = await _postService.GetPostById(id);
            if (post == null) return NotFound();
            return Ok(post);
        }

        // GET /api/posts/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUser(int userId)
        {
            var posts = await _postService.GetPostsByUser(userId);
            return Ok(posts);
        }

        // GET /api/posts/public
        [HttpGet("public")]
        public async Task<IActionResult> GetPublic()
        {
            var posts = await _postService.GetPublicPosts();
            return Ok(posts);
        }

        // GET /api/posts/hashtag/{tag}
        [HttpGet("hashtag/{tag}")]
        public async Task<IActionResult> GetByHashtag(string tag)
        {
            var posts = await _postService.GetByHashtag(tag);
            return Ok(posts);
        }

        // GET /api/posts/search?q=
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            var posts = await _postService.SearchPosts(q);
            return Ok(posts);
        }

        // GET /api/posts/trending?topN=10
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrending([FromQuery] int topN = 10)
        {
            var posts = await _postService.GetTrendingPosts(topN);
            return Ok(posts);
        }

        // PUT /api/posts/{id}
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto dto)
        {
            try
            {
                // Replace this line in both UpdatePost and DeletePost
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

                if (userIdClaim == null) return Unauthorized();
                int requestingUserId = int.Parse(userIdClaim.Value);

                var updated = await _postService.UpdatePost(
                    id, requestingUserId, dto.Content, dto.Hashtags, dto.Visibility);

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

        // DELETE /api/posts/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            try
            {
                // Replace this line in both UpdatePost and DeletePost
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

                if (userIdClaim == null) return Unauthorized();
                int requestingUserId = int.Parse(userIdClaim.Value);

                await _postService.DeletePost(id, requestingUserId);
                return Ok(new { message = "Post deleted successfully." });
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

        // PUT /api/posts/{id}/counts  ← called by Like & Comment services
        [HttpPut("{id}/counts")]
        public async Task<IActionResult> UpdateCount(int id, [FromBody] IncrementCountDto dto)
        {
            try
            {
                if (dto.Field == "LikeCount")
                    await _postService.IncrementLikeCount(id, dto.Delta);
                else if (dto.Field == "CommentCount")
                    await _postService.IncrementCommentCount(id, dto.Delta);
                else if (dto.Field == "ShareCount")
                    await _postService.IncrementShareCount(id, dto.Delta);
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