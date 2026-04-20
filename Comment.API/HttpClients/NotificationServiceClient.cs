namespace Comment.API.HttpClients
{
    public class NotificationServiceClient
    {
        private readonly HttpClient _httpClient;

        public NotificationServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendCommentNotification(
            int postAuthorId, int actorId, int postId)
        {
            var payload = new
            {
                PostAuthorId = postAuthorId,
                ActorId = actorId,
                PostId = postId
            };

            try
            {
                await _httpClient.PostAsJsonAsync(
                    "/api/notifications/comment", payload);
            }
            catch
            {
                // Notification failure should never break comment operation
            }
        }

        public async Task SendMentionNotification(
            int mentionedUserId, int actorId, int postId)
        {
            var payload = new
            {
                MentionedUserId = mentionedUserId,
                ActorId = actorId,
                PostId = postId
            };

            try
            {
                await _httpClient.PostAsJsonAsync(
                    "/api/notifications/mention", payload);
            }
            catch
            {
                // Notification failure should never break comment operation
            }
        }
    }
}