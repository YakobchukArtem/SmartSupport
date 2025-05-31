namespace SmartSupport.Services.Dto;
public class ChatDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public int? Rating { get; set; }
    public List<Guid> MessageIds { get; set; } = new();
}
