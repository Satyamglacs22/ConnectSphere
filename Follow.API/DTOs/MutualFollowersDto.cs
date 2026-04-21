namespace Follow.API.DTOs
{
    public class MutualFollowersDto
    {
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public IList<int> MutualFollowerIds { get; set; } = new List<int>();
        public int MutualCount { get; set; }
    }
}