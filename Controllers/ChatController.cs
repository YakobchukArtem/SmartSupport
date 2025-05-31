using Microsoft.AspNetCore.Mvc;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;

namespace SmartSupport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var chats = await _chatService.GetAllChatsAsync();
            return Ok(chats);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var chat = await _chatService.GetChatByIdAsync(id);
            return Ok(chat);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ChatCreateDto dto)
        {
            await _chatService.CreateChatAsync(dto);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ChatUpdateDto dto)
        {
            await _chatService.UpdateChatAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _chatService.DeleteChatAsync(id);
            return NoContent();
        }
    }
}
