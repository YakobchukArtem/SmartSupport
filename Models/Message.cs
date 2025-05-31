namespace SmartSupport.Models
{
    public class Message
    {
        public Guid Id { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public string Content { get; set; } = string.Empty;

        public bool IsFromAi { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }

}
