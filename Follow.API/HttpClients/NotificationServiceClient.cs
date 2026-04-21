namespace Follow.API.HttpClients
{
    public class NotificationServiceClient
    {
        private readonly HttpClient _httpClient;

        public NotificationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendFollowNotification(
            int targetId, int followerId, string type)
        {
            var payload = new
            {
                TargetId = targetId,
                FollowerId = followerId,
                Type = type  // NEW_FOLLOWER or FOLLOW_REQUEST
            };

            try
            {
                await _httpClient.PostAsJsonAsync(
                    "/api/notifications/follow", payload);
            }
            catch
            {
                // Notification failure should never break follow operation
            }
        }

        public async Task SendFollowAcceptedNotification(
            int followerId, int followeeId)
        {
            var payload = new
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                Type = "FOLLOW_ACCEPTED"
            };

            try
            {
                await _httpClient.PostAsJsonAsync(
                    "/api/notifications/follow", payload);
            }
            catch
            {
                // Notification failure should never break accept operation
            }
        }
    }
}