using System.Text.Json.Serialization;

namespace SmartSupport.Services.Dto
{
    public record CompanyCreateDto
    {
        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public string OwnerId { get; set; } = string.Empty;
    }
}
