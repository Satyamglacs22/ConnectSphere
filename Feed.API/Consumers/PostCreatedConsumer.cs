using Feed.API.Cache;
using Feed.API.Data;
using Feed.API.Entities;
using Feed.API.Events;
using Feed.API.HttpClients;
using MassTransit;

namespace Feed.API.Consumers
{
    // This consumer listens to RabbitMQ
    // When Post API publishes PostCreatedEvent
    // This consumer receives it and fans out to all followers
    public class PostCreatedConsumer : IConsumer<PostCreatedEvent>
    {
        private readonly FeedDbContext _context;
        private readonly FollowServiceClient _followClient;
        private readonly FeedCacheService _cacheService;
        private readonly ILogger<PostCreatedConsumer> _logger;

        public PostCreatedConsumer(
            FeedDbContext context,
            FollowServiceClient followClient,
            FeedCacheService cacheService,
            ILogger<PostCreatedConsumer> logger)
        {
            _context = context;
            _followClient = followClient;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PostCreatedEvent> context)
        {
            var msg = context.Message;

            _logger.LogInformation(
                "PostCreatedEvent received: PostId={PostId}, AuthorId={AuthorId}",
                msg.PostId, msg.AuthorId);

            try
            {
                // Step 1: Get all followers of the author
                var followerIds = await _followClient
                    .GetFollowerIds(msg.AuthorId);

                if (!followerIds.Any())
                {
                    _logger.LogInformation(
                        "No followers for user {AuthorId}", msg.AuthorId);
                    return;
                }

                _logger.LogInformation(
                    "Fanning out to {count} followers", followerIds.Count);

                // Step 2: Create FeedItem for each follower
                var feedItems = followerIds.Select(followerId =>
                    new FeedItemEntity
                    {
                        UserId = followerId,
                        PostId = msg.PostId,
                        AuthorId = msg.AuthorId,
                        CreatedAt = msg.CreatedAt,
                        IsSeen = false
                    }).ToList();

                // Step 3: Batch insert all feed items
                await _context.FeedItems.AddRangeAsync(feedItems);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Feed items created for {count} followers",
                    feedItems.Count);

                // Step 4: Invalidate Redis cache for all followers
                // So they get fresh feed next time
                await _cacheService.InvalidateMultipleFeeds(followerIds);

                _logger.LogInformation(
                    "Cache invalidated for {count} followers",
                    followerIds.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Error consuming PostCreatedEvent: {msg}", ex.Message);
                throw; // MassTransit will retry
            }
        }
    }
}