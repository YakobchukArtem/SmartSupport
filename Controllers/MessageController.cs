using Microsoft.AspNetCore.Mvc;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;

namespace SmartSupport.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessageController(IMessageService messageService) : ControllerBase
    {

        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetAllByChatId(Guid chatId)
        {
            var messages = await messageService.GetAllMessagesAsync(chatId);
            return Ok(messages);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var message = await messageService.GetMessageByIdAsync(id);
            if (message == null)
                return NotFound();

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MessageCreateDto dto)
        {
            var result = await messageService.CreateMessageAsync(dto);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MessageUpdateDto dto)
        {
            try
            {
                await messageService.UpdateMessageAsync(id, dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("test")]
        public async Task<IActionResult> Test([FromBody] TestAiRequest request)
        {
            return Ok(await messageService.TestAiAsync(request));
        }


    }
}
