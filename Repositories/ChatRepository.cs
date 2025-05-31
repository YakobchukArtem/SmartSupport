using Microsoft.EntityFrameworkCore;
using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;

namespace SmartSupport.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly SmartSupportDbContext _context;

    public ChatRepository(SmartSupportDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Chat>> GetAllAsync()
    {
        return await _context.Chats
            .Include(c => c.Messages)
            .Include(c => c.Company)
            .ToListAsync();
    }

    public async Task<Chat?> GetByIdAsync(Guid id)
    {
        return await _context.Chats
            .Include(c => c.Messages)
            .Include(c => c.Company)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Chat chat)
    {
        await _context.Chats.AddAsync(chat);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Chat chat)
    {
        _context.Chats.Update(chat);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var chat = await _context.Chats.FindAsync(id);
        if (chat != null)
        {
            _context.Chats.Remove(chat);
            await _context.SaveChangesAsync();
        }
    }
}
