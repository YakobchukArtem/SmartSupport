using SmartSupport.Models;

namespace SmartSupport.Repositories.Interfaces;

public interface IChatRepository
{
    Task<IEnumerable<Chat>> GetAllAsync();
    Task<Chat?> GetByIdAsync(Guid id);
    Task AddAsync(Chat chat);
    Task UpdateAsync(Chat chat);
    Task DeleteAsync(Guid id);
}

