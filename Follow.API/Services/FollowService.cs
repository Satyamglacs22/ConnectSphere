using Follow.API.DTOs;
using Follow.API.Entities;
using Follow.API.HttpClients;
using Follow.API.Repositories.Interfaces;
using Follow.API.Services.Interfaces;

namespace Follow.API.Services
{
    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _repo;
        private readonly AuthServiceClient _authClient;
        private readonly NotificationServiceClient _notifClient;
        private readonly ILogger<FollowService> _logger;

        public FollowService(
            IFollowRepository repo,
            AuthServiceClient authClient,
            NotificationServiceClient notifClient,
            ILogger<FollowService> logger)
        {
            _repo = repo;
            _authClient = authClient;
            _notifClient = notifClient;
            _logger = logger;
        }

        public async Task<FollowResponseDto> FollowUser(int followerId, int followeeId)
        {
            // Check if already following
            var existing = await _repo.FindByFollowerAndFollowee(followerId, followeeId);
            if (existing != null)
            {
                return MapToDto(existing, "Already following this user.");
            }

            // Check if target user is private
            bool isPrivate = await _authClient.IsUserPrivate(followeeId);

            var follow = new FollowEntity
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                // Public account → ACCEPTED immediately
                // Private account → PENDING until accepted
                Status = isPrivate ? "PENDING" : "ACCEPTED",
                CreatedAt = DateTime.UtcNow
            };

            var created = await _repo.Create(follow);

            // Only update counters if immediately accepted (public account)
            if (!isPrivate)
            {
                await _authClient.UpdateCounters(followerId, "FollowingCount", +1);
                await _authClient.UpdateCounters(followeeId, "FollowerCount", +1);
            }

            // Send notification
            string notifType = isPrivate ? "FOLLOW_REQUEST" : "NEW_FOLLOWER";
            await _notifClient.SendFollowNotification(followeeId, followerId, notifType);

            string message = isPrivate
                ? "Follow request sent. Waiting for approval."
                : "Successfully followed user.";

            return MapToDto(created, message);
        }

        public async Task UnfollowUser(int followerId, int followeeId)
        {
            var follow = await _repo.FindByFollowerAndFollowee(followerId, followeeId);
            if (follow == null) throw new Exception("Follow relationship not found.");

            await _repo.DeleteByFollowId(follow.FollowId);

            // Only decrement if was ACCEPTED (not pending)
            if (follow.Status == "ACCEPTED")
            {
                await _authClient.UpdateCounters(followerId, "FollowingCount", -1);
                await _authClient.UpdateCounters(followeeId, "FollowerCount", -1);
            }
        }

        public async Task<FollowResponseDto> AcceptFollowRequest(int followId)
        {
            var follow = await _repo.FindByFollowId(followId);
            if (follow == null) throw new Exception("Follow request not found.");

            if (follow.Status != "PENDING")
                throw new Exception("This request is not pending.");

            // Update status to ACCEPTED
            follow.Status = "ACCEPTED";
            var updated = await _repo.Update(follow);

            // Now update counters — was pending before so no counters yet
            await _authClient.UpdateCounters(follow.FollowerId, "FollowingCount", +1);
            await _authClient.UpdateCounters(follow.FolloweeId, "FollowerCount", +1);

            // Send FOLLOW_ACCEPTED notification to the follower
            await _notifClient.SendFollowAcceptedNotification(
                follow.FollowerId, follow.FolloweeId);

            return MapToDto(updated, "Follow request accepted.");
        }

        public async Task RejectFollowRequest(int followId)
        {
            var follow = await _repo.FindByFollowId(followId);
            if (follow == null) throw new Exception("Follow request not found.");

            if (follow.Status != "PENDING")
                throw new Exception("This request is not pending.");

            // Hard delete — no counter change needed
            await _repo.DeleteByFollowId(followId);
        }

        public async Task<IList<FollowEntity>> GetFollowers(int userId)
        {
            return await _repo.FindFollowersByUserId(userId);
        }

        public async Task<IList<FollowEntity>> GetFollowing(int userId)
        {
            return await _repo.FindFollowingByUserId(userId);
        }

        public async Task<IList<FollowEntity>> GetPendingRequests(int userId)
        {
            return await _repo.FindPendingRequests(userId);
        }

        public async Task<bool> IsFollowing(int followerId, int followeeId)
        {
            return await _repo.IsFollowing(followerId, followeeId);
        }

        public async Task<int> GetFollowerCount(int userId)
        {
            return await _repo.CountFollowers(userId);
        }

        public async Task<int> GetFollowingCount(int userId)
        {
            return await _repo.CountFollowing(userId);
        }

        public async Task<IList<int>> GetFollowingIds(int userId)
        {
            return await _repo.FindFollowingIds(userId);
        }

        public async Task<IList<int>> GetFollowerIds(int userId)
        {
            return await _repo.FindFollowerIds(userId);
        }

        public async Task<MutualFollowersDto> GetMutualFollowers(int userAId, int userBId)
        {
            // Get follower IDs of both users
            var followersOfA = await _repo.FindFollowerIds(userAId);
            var followersOfB = await _repo.FindFollowerIds(userBId);

            // Intersection — users who follow BOTH
            var mutualIds = followersOfA.Intersect(followersOfB).ToList();

            return new MutualFollowersDto
            {
                UserAId = userAId,
                UserBId = userBId,
                MutualFollowerIds = mutualIds,
                MutualCount = mutualIds.Count
            };
        }

        // ── Private Helper ─────────────────────────────────────────────────
        private FollowResponseDto MapToDto(FollowEntity follow, string message = "")
        {
            return new FollowResponseDto
            {
                FollowId = follow.FollowId,
                FollowerId = follow.FollowerId,
                FolloweeId = follow.FolloweeId,
                Status = follow.Status,
                CreatedAt = follow.CreatedAt,
                Message = message
            };
        }
    }
}