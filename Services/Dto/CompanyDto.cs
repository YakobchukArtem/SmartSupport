namespace SmartSupport.Services.Dto
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? DocumentationUrl { get; set; }
        public string DocumentationText { get; set; }
        public string[] Categories { get; set; } = [];
        public string OwnerId { get; set; } = string.Empty;
        public List<Guid> ChatIds { get; set; } = new();
    }
}
