using Post.API.Entities;

namespace Post.API.Services.Interfaces
{
    public interface IPostService
    {
        Task<PostEntity> CreatePost(PostEntity post);
        Task<PostEntity?> GetPostById(int id);
        Task<IList<PostEntity>> GetPostsByUser(int userId);
        Task<IList<PostEntity>> GetFeed(int userId, IList<int> followingIds);
        Task<IList<PostEntity>> GetPublicPosts();
        Task<PostEntity> UpdatePost(int id, int requestingUserId, string content, string? hashtags, string visibility);
        Task DeletePost(int id, int requestingUserId);
        Task<IList<PostEntity>> GetByHashtag(string tag);
        Task<IList<PostEntity>> SearchPosts(string q);
        Task<IList<PostEntity>> GetTrendingPosts(int topN);
        Task IncrementLikeCount(int id, int delta);
        Task IncrementCommentCount(int id, int delta);
        Task IncrementShareCount(int id, int delta);
    }
}