using SmartSupport.Services.Dto;

namespace SmartSupport.Services.Interfaces;
public interface IChatService
{
    Task<IEnumerable<ChatDto>> GetAllChatsAsync();
    Task<ChatDto> GetChatByIdAsync(Guid id);
    Task CreateChatAsync(ChatCreateDto chatDto);
    Task UpdateChatAsync(Guid id, ChatUpdateDto chatDto);
    Task DeleteChatAsync(Guid id);
}
