using ClosedXML.Excel;
using SmartSupport.Models;
using SmartSupport.Repositories.Interfaces;
using SmartSupport.Services.Dto;
using SmartSupport.Services.Interfaces;
using System.Diagnostics;

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

            var response = await aiService.GetAiResponseAsync(messageDto.Content, documentation, messageDto.IsLocal);

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
                Message = response.message + $"  Refer to {response.reference} for more information.",
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

        public class TestAiResult
        {
            public int ProcessedRows { get; set; }
            public double ElapsedSeconds { get; set; }
        }

        public async Task<TestAiResult> TestAiAsync(TestAiRequest request)
        {
            var stopwatch = Stopwatch.StartNew();

            var documentation = (await companyService.GetCompanyByIdAsync(request.CompanyId))?.DocumentationText;

            var cleanPath = CleanPath(request.InputFilePath);

            if (!File.Exists(cleanPath))
                throw new FileNotFoundException("Файл не знайдено", cleanPath);

            using var workbook = new XLWorkbook(cleanPath);
            var worksheet = workbook.Worksheet(1);

            var headerCells = worksheet.Row(1).Cells().ToDictionary(c => c.GetString(), c => c.Address.ColumnNumber);

            int questionCol = headerCells["Question"];
            int botAnswerCol = headerCells["BotAnswer"];
            int botReferenceCol = headerCells["BotReference"];

            var usedRows = worksheet.RangeUsed().RowsUsed();
            var rowsToProcess = usedRows.Where(r => r.RowNumber() >= request.StartRow);

            int processedCount = 0;

            var tasks = rowsToProcess.Select(async row =>
            {
                var question = row.Cell(questionCol).GetString();
                if (string.IsNullOrWhiteSpace(question))
                    return;

                var response = await aiService.GetAiResponseAsync(question, documentation, request.IsLocal);

                row.Cell(botAnswerCol).Value = response.message ?? "";
                row.Cell(botReferenceCol).Value = response.reference ?? "";

                processedCount++;
            });

            await Task.WhenAll(tasks);

            workbook.SaveAs(request.InputFilePath);

            stopwatch.Stop();

            return new TestAiResult
            {
                ProcessedRows = processedCount,
                ElapsedSeconds = stopwatch.Elapsed.TotalSeconds
            };
        }

        string CleanPath(string path)
        {
            return new string(path.Where(c => !char.IsControl(c) && c != '\u202A').ToArray());
        }

    }

}
