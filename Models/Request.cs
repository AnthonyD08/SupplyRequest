namespace WebApi.Models
{
    public class Request
    {
        public int RequestId { get; set; }
        public int Employee { get; set; }
        public int? Accountant { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public bool Active { get; set; } = true;
    }
}
