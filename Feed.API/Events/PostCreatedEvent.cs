namespace Feed.API.Events
{
    // Same contract as Post.API/Events/PostCreatedEvent.cs
    // Must match EXACTLY — same property names and types
    // MassTransit matches by contract, not by namespace
    public class PostCreatedEvent
    {
        public int PostId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}