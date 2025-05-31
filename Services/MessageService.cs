using ClosedXML.Excel;
using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;

namespace SmartSupport.Services
{
    public class MessageService(IMessageRepository messageRepository,
        IChatService chatService,
        IAiService aiService,
        ICompanyService companyService) : IMessageService
    {
        public async Task<IEnumerable<MessageDto>> GetAllMessagesAsync(Guid chatId)
        {
            var messages = await messageRepository.GetAllAsync(chatId);
            return messages.Select(m => new MessageDto
            {
                Id = m.Id,
                ChatId = m.ChatId,
                Content = m.Content,
                IsFromAi = m.IsFromAi,
                CreatedAtUtc = m.CreatedAtUtc
            });
        }

        public async Task<MessageDto?> GetMessageByIdAsync(Guid id)
        {
            var message = await messageRepository.GetByIdAsync(id);
            if (message == null)
                return null;

            return new MessageDto
            {
                Id = message.Id,
                ChatId = message.ChatId,
                Content = message.Content,
                IsFromAi = message.IsFromAi,
                CreatedAtUtc = message.CreatedAtUtc
            };
        }

        public async Task<MessageResult> CreateMessageAsync(MessageCreateDto messageDto)
        {
            Guid chatId;

            if (!messageDto.ChatId.HasValue)
            {
                chatId = Guid.NewGuid();
                await chatService.CreateChatAsync(new()
                {
                    ChatId = chatId,
                    CompanyId = messageDto.CompanyId
                });
            }
            else
            {
                chatId = messageDto.ChatId.Value;
            }

            await messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                Content = messageDto.Content,
                IsFromAi = false,
                CreatedAtUtc = DateTime.UtcNow
            });

            var documentation = (await companyService.GetCompanyByIdAsync(messageDto.CompanyId))?.DocumentationText;

            var response = await aiService.GetAiResponseAsync(messageDto.Content, documentation);

            await messageRepository.AddAsync(new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                Content = response.message + " " + response.reference,
                IsFromAi = true,
                CreatedAtUtc = DateTime.UtcNow
            });

            return new()
            {
                Message = response.message + Environment.NewLine + $"Refer to {response.reference} for more information.",
                ChatId = chatId
            };
        }

        public async Task UpdateMessageAsync(Guid id, MessageUpdateDto messageDto)
        {
            var message = await messageRepository.GetByIdAsync(id);
            if (message == null)
                throw new KeyNotFoundException("Message not found");

            message.Content = messageDto.Content;

            await messageRepository.UpdateAsync(message);
        }

        public async Task TestAiAsync(TestAiRequest request)
        {
            var documentation = (await companyService.GetCompanyByIdAsync(request.CompanyId))?.DocumentationText;

            using var workbook = new XLWorkbook(request.InputFilePath);
            var worksheet = workbook.Worksheet(1);

            // Визначаємо номери колонок за назвами в заголовку (рядок 1)
            var headerCells = worksheet.Row(1).Cells().ToDictionary(c => c.GetString(), c => c.Address.ColumnNumber);

            int questionCol = headerCells["Question"];
            int botAnswerCol = headerCells["BotAnswer"];
            int botReferenceCol = headerCells["BotReference"];

            // Визначаємо усі рядки, які використовуються у діапазоні
            var usedRows = worksheet.RangeUsed().RowsUsed();

            // Фільтруємо рядки, починаючи з потрібного startRow (враховуючи, що рядок 1 - це заголовок)
            var rowsToProcess = usedRows.Where(r => r.RowNumber() >= request.StartRow);

            var tasks = rowsToProcess.Select(async row =>
            {
                var question = row.Cell(questionCol).GetString();
                if (string.IsNullOrWhiteSpace(question))
                    return;

                var response = await aiService.GetAiResponseAsync(question, documentation);

                row.Cell(botAnswerCol).Value = response.message ?? "";
                row.Cell(botReferenceCol).Value = response.reference ?? "";
            });

            await Task.WhenAll(tasks);

            workbook.SaveAs(request.InputFilePath);
        }
    }

}
