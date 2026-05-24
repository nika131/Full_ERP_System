namespace NexusERP.Api.DTOs
{
    public class DashboardResponseDto
    {
        public decimal TotalValue { get; set; }     
        public decimal TotalCost { get; set; }
        public decimal TotalProfit {  get; set; }   
        public int LowStockCount { get; set; }
        public decimal MarginPrecentage { get; set; }
        public string InventoryHealth { get; set; } = string.Empty;
    }
}
