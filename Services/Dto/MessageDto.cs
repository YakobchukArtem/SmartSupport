namespace SmartSupport.Services.Dto
{
    public class MessageDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public string Content { get; set; } = string.Empty;
        public bool IsFromAi { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
