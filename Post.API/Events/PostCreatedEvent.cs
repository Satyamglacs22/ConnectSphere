namespace Post.API.Events
{
    // This message is published to RabbitMQ via MassTransit
    // Feed.API will consume this to fan-out the post to followers' feeds
    public class PostCreatedEvent
    {
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}