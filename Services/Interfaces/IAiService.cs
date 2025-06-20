﻿namespace SmartSupport.Services.Interfaces
{
    public interface IAiService
    {
        Task<InnerMessage> GetAiResponseAsync(string question, string documentation, bool Islocal);
        Task<string> GetCategoriesAsync(string question, string documentation, bool Islocal);
    }
}
