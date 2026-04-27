namespace ConnectSphere.Tests.Helpers
{
    public static class TestData
    {
        // Unique test users
        public static object User1 => new
        {
            UserName = $"testuser1_{Guid.NewGuid():N}",
            FullName = "Test User One",
            Email = $"testuser1_{Guid.NewGuid():N}@test.com",
            Password = "Password123!"
        };

        public static object User2 => new
        {
            UserName = $"testuser2_{Guid.NewGuid():N}",
            FullName = "Test User Two",
            Email = $"testuser2_{Guid.NewGuid():N}@test.com",
            Password = "Password123!"
        };

        public static object CreatePost(int userId) => new
        {
            UserId = userId,
            Content = "This is a test post!",
            Visibility = "PUBLIC",
            Hashtags = "#test,#integration"
        };

        public static object CreateComment(int postId, int userId) => new
        {
            PostId = postId,
            UserId = userId,
            Content = "This is a test comment!",
            ParentCommentId = (int?)null
        };

        public static object ToggleLike(int userId, int targetId,
            string targetType = "POST") => new
        {
            UserId = userId,
            TargetId = targetId,
            TargetType = targetType
        };

        public static object FollowRequest(
            int followerId, int followeeId) => new
        {
            FollowerId = followerId,
            FolloweeId = followeeId
        };
    }
}
