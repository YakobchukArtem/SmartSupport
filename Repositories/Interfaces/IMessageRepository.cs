using SmartSupport.Models;

namespace SmartSupport.Repositories.Interfaces;
public interface IMessageRepository
{
    Task<IEnumerable<Message>> GetAllAsync(Guid chatId);
    Task<Message?> GetByIdAsync(Guid id);
    Task AddAsync(Message message);
    Task UpdateAsync(Message message);
    Task DeleteAsync(Guid id);
}

