using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;

namespace SmartSupport.Services;

public class ChatService(IChatRepository chatRepository) : IChatService
{
    public async Task<IEnumerable<ChatDto>> GetAllChatsAsync()
    {
        var chats = await chatRepository.GetAllAsync();
        return chats.Select(chat => new ChatDto
        {
            Id = chat.Id,
            CompanyId = chat.CompanyId,
            Rating = chat.Rating,
            MessageIds = chat.Messages.Select(m => m.Id).ToList()
        });
    }

    public async Task<ChatDto> GetChatByIdAsync(Guid id)
    {
        var chat = await chatRepository.GetByIdAsync(id);
        if (chat == null)
            throw new KeyNotFoundException($"Chat with ID {id} not found.");

        return new ChatDto
        {
            Id = chat.Id,
            CompanyId = chat.CompanyId,
            Rating = chat.Rating,
            MessageIds = chat.Messages.Select(m => m.Id).ToList()
        };
    }

    public async Task CreateChatAsync(ChatCreateDto chatDto)
    {
        var chat = new Chat
        {
            Id = chatDto.ChatId ?? Guid.NewGuid(),
            CompanyId = chatDto.CompanyId
        };

        await chatRepository.AddAsync(chat);
    }

    public async Task UpdateChatAsync(Guid id, ChatUpdateDto chatDto)
    {
        var chat = await chatRepository.GetByIdAsync(id);
        if (chat == null)
            throw new KeyNotFoundException($"Chat with ID {id} not found.");

        chat.Rating = chatDto.Rating;
        await chatRepository.UpdateAsync(chat);
    }

    public async Task DeleteChatAsync(Guid id)
    {
        var chat = await chatRepository.GetByIdAsync(id);
        if (chat == null)
            throw new KeyNotFoundException($"Chat with ID {id} not found.");

        await chatRepository.DeleteAsync(id);
    }
}

