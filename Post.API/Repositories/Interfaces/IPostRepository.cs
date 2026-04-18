using Post.API.Entities;

namespace Post.API.Repositories.Interfaces
{
    public interface IPostRepository
    {
        Task<PostEntity?> FindByPostId(int id);
        Task<IList<PostEntity>> FindByUserId(int userId);
        Task<IList<PostEntity>> FindFeedForUser(int userId, IList<int> followingIds);
        Task<IList<PostEntity>> FindByHashtag(string hashtag);
        Task<IList<PostEntity>> FindPublic();
        Task<IList<PostEntity>> SearchPosts(string q);
        Task IncrementCount(int postId, string field, int delta);
        Task<IList<PostEntity>> FindTrending(int topN);
        Task DeleteByPostId(int id);
        Task<PostEntity> Create(PostEntity post);
        Task<PostEntity> Update(PostEntity post);
    }
}