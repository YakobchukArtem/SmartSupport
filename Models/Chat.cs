namespace SmartSupport.Models;
public class Chat
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }
    public Company Company { get; set; }

    public int? Rating { get; set; }

    public List<Message> Messages { get; set; } = new();
}

