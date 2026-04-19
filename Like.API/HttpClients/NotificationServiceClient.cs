namespace Like.API.HttpClients
{
    // Calls Notification.API to send like notification
    public class NotificationServiceClient
    {
        private readonly HttpClient _httpClient;

        public NotificationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendLikeNotification(
            int recipientId, int actorId, int targetId, string targetType)
        {
            var payload = new
            {
                RecipientId = recipientId,
                ActorId = actorId,
                TargetId = targetId,
                TargetType = targetType
            };

            try
            {
                await _httpClient.PostAsJsonAsync("/api/notifications/like", payload);
            }
            catch
            {
                // Notification failure should never break the like operation
            }
        }
    }
}