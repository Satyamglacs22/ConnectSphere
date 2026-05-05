namespace Follow.API.DTOs
{
    public class FollowSuggestionDto
    {
        public int SuggestedUserId { get; set; }
        public IList<int> MutualFriendIds { get; set; } = new List<int>();
        public int MutualCount { get; set; }
    }
}
