using Microsoft.AspNetCore.Identity;

namespace SmartSupport.Models;

public class Company
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? DocumentationUrl { get; set; }

    public string[] Categories { get; set; } = [];

    public string OwnerId { get; set; }
    public IdentityUser Owner { get; set; } = null!;

    public List<Chat> Chats { get; set; } = [];
}



