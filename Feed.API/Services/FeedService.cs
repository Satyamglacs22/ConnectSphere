using Feed.API.Cache;
using Feed.API.DTOs;
using Feed.API.Entities;
using Feed.API.HttpClients;
using Feed.API.Repositories.Interfaces;
using Feed.API.Services.Interfaces;

namespace Feed.API.Services
{
    public class FeedService : IFeedService
    {
        private readonly IFeedRepository _repo;
        private readonly FeedCacheService _cacheService;
        private readonly PostServiceClient _postClient;
        private readonly ILogger<FeedService> _logger;

        public FeedService(
            IFeedRepository repo,
            FeedCacheService cacheService,
            PostServiceClient postClient,
            ILogger<FeedService> logger)
        {
            _repo = repo;
            _cacheService = cacheService;
            _postClient = postClient;
            _logger = logger;
        }

        public async Task<IList<FeedResponseDto>> GetFeed(
            int userId, int page, int pageSize)
        {
            // ── Cache Aside Pattern ────────────────────────────────────────
            // Step 1: Check Redis cache first
            if (page == 1)
            {
                var cached = await _cacheService.GetFeed(userId);
                if (cached != null)
                {
                    _logger.LogInformation(
                        "Returning cached feed for user {userId}", userId);

                    // Enrich with post details and return
                    return await EnrichFeedItems(cached);
                }
            }

            // Step 2: Cache miss → get from database
            _logger.LogInformation(
                "Cache miss → fetching from DB for user {userId}", userId);

            var feedItems = await _repo.GetFeedByUserId(
                userId, page, pageSize);

            // Step 3: Store in cache for next time (only page 1)
            if (page == 1 && feedItems.Any())
            {
                await _cacheService.SetFeed(userId, feedItems.ToList());
            }

            // Step 4: Enrich with post details and return
            return await EnrichFeedItems(feedItems.ToList());
        }

        public async Task<IList<FeedItemEntity>> GetUnseenFeed(int userId)
        {
            return await _repo.GetUnseenFeed(userId);
        }

        public async Task<int> GetUnseenCount(int userId)
        {
            return await _repo.GetUnseenCount(userId);
        }

        public async Task MarkFeedAsSeen(int userId)
        {
            await _repo.MarkAsSeen(userId);

            // Invalidate cache — feed changed
            await _cacheService.InvalidateFeed(userId);
        }

        // ── Private Helper ─────────────────────────────────────────────────
        // Enrich feed items with post details from Post API
        private async Task<IList<FeedResponseDto>> EnrichFeedItems(
            List<FeedItemEntity> feedItems)
        {
            var result = new List<FeedResponseDto>();

            foreach (var item in feedItems)
            {
                // Get post details from Post API
                var post = await _postClient.GetPostById(item.PostId);

                result.Add(new FeedResponseDto
                {
                    FeedItemId = item.FeedItemId,
                    UserId = item.UserId,
                    PostId = item.PostId,
                    AuthorId = item.AuthorId,
                    IsSeen = item.IsSeen,
                    CreatedAt = item.CreatedAt,

                    // Post details
                    Content = post?.Content,
                    MediaUrl = post?.MediaUrl,
                    Hashtags = post?.Hashtags,
                    LikeCount = post?.LikeCount ?? 0,
                    CommentCount = post?.CommentCount ?? 0
                });
            }

            return result;
        }
    }
}