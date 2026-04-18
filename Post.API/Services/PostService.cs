using MassTransit;
using Post.API.Entities;
using Post.API.Events;
using Post.API.HttpClients;
using Post.API.Repositories.Interfaces;
using Post.API.Services.Interfaces;

namespace Post.API.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _repo;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly AuthServiceClient _authClient;
        private readonly ILogger<PostService> _logger;

        public PostService(
            IPostRepository repo,
            IPublishEndpoint publishEndpoint,
            AuthServiceClient authClient,
            ILogger<PostService> logger)
        {
            _repo = repo;
            _publishEndpoint = publishEndpoint;
            _authClient = authClient;
            _logger = logger;
        }

        public async Task<PostEntity> CreatePost(PostEntity post)
        {
            post.CreatedAt = DateTime.UtcNow;

            var created = await _repo.Create(post);

            await _publishEndpoint.Publish<PostCreatedEvent>(new
            {
                PostId = created.PostId,
                AuthorId = created.UserId,
                CreatedAt = created.CreatedAt
            });

            try
            {
                await _authClient.UpdatePostCount(created.UserId, +1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to update PostCount on Auth service: {msg}", ex.Message);
            }

            return created;
        }

        public async Task<PostEntity?> GetPostById(int id)
        {
            return await _repo.FindByPostId(id);
        }

        public async Task<IList<PostEntity>> GetPostsByUser(int userId)
        {
            return await _repo.FindByUserId(userId);
        }

        public async Task<IList<PostEntity>> GetFeed(int userId, IList<int> followingIds)
        {
            return await _repo.FindFeedForUser(userId, followingIds);
        }

        public async Task<IList<PostEntity>> GetPublicPosts()
        {
            return await _repo.FindPublic();
        }

        public async Task<PostEntity> UpdatePost(int id, int requestingUserId, string content, string? hashtags, string visibility)
        {
            var post = await _repo.FindByPostId(id);
            if (post == null) throw new Exception("Post not found.");

            if (post.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You can only edit your own posts.");

            post.Content = content;
            post.Hashtags = hashtags;
            post.Visibility = visibility;
            post.UpdatedAt = DateTime.UtcNow;

            return await _repo.Update(post);
        }

        public async Task DeletePost(int id, int requestingUserId)
        {
            var post = await _repo.FindByPostId(id);
            if (post == null) throw new Exception("Post not found.");

            if (post.UserId != requestingUserId)
                throw new UnauthorizedAccessException("You can only delete your own posts.");

            await _repo.DeleteByPostId(id);

            try
            {
                await _authClient.UpdatePostCount(post.UserId, -1);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to decrement PostCount on Auth service: {msg}", ex.Message);
            }
        }

        public async Task<IList<PostEntity>> GetByHashtag(string tag)
        {
            return await _repo.FindByHashtag(tag);
        }

        public async Task<IList<PostEntity>> SearchPosts(string q)
        {
            return await _repo.SearchPosts(q);
        }

        public async Task<IList<PostEntity>> GetTrendingPosts(int topN)
        {
            return await _repo.FindTrending(topN);
        }

        public async Task IncrementLikeCount(int id, int delta)
        {
            await _repo.IncrementCount(id, "LikeCount", delta);
        }

        public async Task IncrementCommentCount(int id, int delta)
        {
            await _repo.IncrementCount(id, "CommentCount", delta);
        }

        public async Task IncrementShareCount(int id, int delta)
        {
            await _repo.IncrementCount(id, "ShareCount", delta);
        }
    }
}