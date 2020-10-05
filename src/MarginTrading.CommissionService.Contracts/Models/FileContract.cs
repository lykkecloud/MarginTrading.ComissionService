namespace MarginTrading.CommissionService.Contracts.Models
{
    public class FileContract
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public byte[] Content { get; set; }
    }
}
