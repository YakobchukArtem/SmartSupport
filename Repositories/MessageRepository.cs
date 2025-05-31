using Microsoft.EntityFrameworkCore;
using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;

namespace SmartSupport.Repositories;
public class MessageRepository : IMessageRepository
{
    private readonly SmartSupportDbContext _context;

    public MessageRepository(SmartSupportDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Message>> GetAllAsync(Guid chatId)
    {
        return await _context.Messages
            .Where(m => m.ChatId == chatId)
            .Include(m => m.Chat)
            .OrderBy(m => m.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Message?> GetByIdAsync(Guid id)
    {
        return await _context.Messages
            .Include(m => m.Chat)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task AddAsync(Message message)
    {
        await _context.Messages.AddAsync(message);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Message message)
    {
        _context.Messages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var message = await _context.Messages.FindAsync(id);
        if (message != null)
        {
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }
}