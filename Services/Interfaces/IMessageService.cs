using SmartSupport.Services.Dto;
using static SmartSupport.Services.MessageService;

namespace SmartSupport.Services.Interfaces;
public interface IMessageService
{
    Task<IEnumerable<MessageDto>> GetAllMessagesAsync(Guid chatId);
    Task<MessageDto?> GetMessageByIdAsync(Guid id);
    Task<MessageResult> CreateMessageAsync(MessageCreateDto messageDto);
    Task UpdateMessageAsync(Guid id, MessageUpdateDto messageDto);
    Task<TestAiResult> TestAiAsync(TestAiRequest request);
}
