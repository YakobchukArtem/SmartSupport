namespace SmartSupport.Services.Dto
{
    public class TestAiRequest
    {
        public Guid CompanyId { get; set; }
        public string InputFilePath { get; set; }

        public int StartRow { get; set; }

        public bool IsLocal { get; set; }
    }
}
