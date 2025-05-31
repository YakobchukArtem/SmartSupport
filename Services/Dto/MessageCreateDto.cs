using System.Text.Json.Serialization;

namespace SmartSupport.Services.Dto
{
    public class MessageCreateDto
    {
        public Guid? ChatId { get; set; }

        public Guid CompanyId { get; set; }
        public string Content { get; set; } = string.Empty;

        [JsonIgnore]
        public bool IsFromAi { get; set; }
    }
}
