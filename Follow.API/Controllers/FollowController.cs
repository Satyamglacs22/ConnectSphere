using Follow.API.DTOs;
using Follow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Follow.API.Controllers
{
    [ApiController]
    [Route("api/follows")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        // POST /api/follows
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FollowUser([FromBody] FollowRequestDto dto)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int followerId = int.Parse(userIdClaim.Value);

                var result = await _followService.FollowUser(
                    followerId, dto.FolloweeId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/follows/{followeeId}
        [Authorize]
        [HttpDelete("{followeeId}")]
        public async Task<IActionResult> UnfollowUser(int followeeId)
        {
            try
            {
                var userIdClaim = User.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int followerId = int.Parse(userIdClaim.Value);

                await _followService.UnfollowUser(followerId, followeeId);
                return Ok(new { message = "Unfollowed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/follows/{followId}/accept
        [Authorize]
        [HttpPut("{followId}/accept")]
        public async Task<IActionResult> AcceptRequest(int followId)
        {
            try
            {
                var result = await _followService.AcceptFollowRequest(followId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT /api/follows/{followId}/reject
        [Authorize]
        [HttpPut("{followId}/reject")]
        public async Task<IActionResult> RejectRequest(int followId)
        {
            try
            {
                await _followService.RejectFollowRequest(followId);
                return Ok(new { message = "Follow request rejected." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/follows/{userId}/followers
        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(int userId)
        {
            try
            {
                var followers = await _followService.GetFollowers(userId);
                return Ok(followers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving followers", details = ex.Message });
            }
        }

        // GET /api/follows/{userId}/following
        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetFollowing(int userId)
        {
            try
            {
                var following = await _followService.GetFollowing(userId);
                return Ok(following);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving following", details = ex.Message });
            }
        }

        // GET /api/follows/{userId}/pending
        [Authorize]
        [HttpGet("{userId}/pending")]
        public async Task<IActionResult> GetPending(int userId)
        {
            try
            {
                var pending = await _followService.GetPendingRequests(userId);
                return Ok(pending);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving pending requests", details = ex.Message });
            }
        }

        // GET /api/follows/isFollowing/{followerId}/{followeeId}
        [Authorize]
        [HttpGet("isFollowing/{followerId}/{followeeId}")]
        public async Task<IActionResult> IsFollowing(int followerId, int followeeId)
        {
            var isFollowing = await _followService.IsFollowing(followerId, followeeId);
            return Ok(new { followerId, followeeId, isFollowing });
        }

        // GET /api/follows/status/{followerId}/{followeeId}
        [Authorize]
        [HttpGet("status/{followerId}/{followeeId}")]
        public async Task<IActionResult> GetFollowStatus(int followerId, int followeeId)
        {
            var entity = await _followService.GetFollowRelationship(followerId, followeeId);
            string status = entity?.Status ?? "NONE";
            return Ok(new { followerId, followeeId, status });
        }

        // GET /api/follows/mutual/{userAId}/{userBId}
        [HttpGet("mutual/{userAId}/{userBId}")]
        public async Task<IActionResult> GetMutual(int userAId, int userBId)
        {
            var mutual = await _followService.GetMutualFollowers(userAId, userBId);
            return Ok(mutual);
        }

        // GET /api/follows/{userId}/followerIds
        [HttpGet("{userId}/followerIds")]
        public async Task<IActionResult> GetFollowerIds(int userId)
        {
            var ids = await _followService.GetFollowerIds(userId);
            return Ok(new { userId, followerIds = ids });
        }

        // GET /api/follows/{userId}/followingIds
        [HttpGet("{userId}/followingIds")]
        public async Task<IActionResult> GetFollowingIds(int userId)
        {
            var ids = await _followService.GetFollowingIds(userId);
            return Ok(new { userId, followingIds = ids });
        }

        // ── Blocks ──────────────────────────────────

        // POST /api/follows/block/{blockedId}
        [Authorize]
        [HttpPost("block/{blockedId}")]
        public async Task<IActionResult> BlockUser(int blockedId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int blockerId = int.Parse(userIdClaim.Value);

                await _followService.BlockUser(blockerId, blockedId);
                return Ok(new { message = "User blocked." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE /api/follows/block/{blockedId}
        [Authorize]
        [HttpDelete("block/{blockedId}")]
        public async Task<IActionResult> UnblockUser(int blockedId)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int blockerId = int.Parse(userIdClaim.Value);

                await _followService.UnblockUser(blockerId, blockedId);
                return Ok(new { message = "User unblocked." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/follows/isBlocked/{userId}
        [Authorize]
        [HttpGet("isBlocked/{userId}")]
        public async Task<IActionResult> IsBlocked(int userId)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);

            var blocked = await _followService.IsBlocked(currentUserId, userId);
            return Ok(new { userId, isBlocked = blocked });
        }

        // GET /api/follows/blocked
        [Authorize]
        [HttpGet("blocked")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();
            int currentUserId = int.Parse(userIdClaim.Value);

            var blockedIds = await _followService.GetBlockedUsers(currentUserId);
            return Ok(blockedIds);
        }

        // GET /api/follows/suggestions?count=5
        [Authorize]
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetSuggestions([FromQuery] int count = 5)
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int currentUserId = int.Parse(userIdClaim.Value);

                var suggestions = await _followService.GetFollowSuggestions(currentUserId, count);
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving follow suggestions", details = ex.Message });
            }
        }
    }
}