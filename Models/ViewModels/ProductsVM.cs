using EquityHarbour.DTOs.Investments;

namespace EquityHarbour.Models.ViewModels
{
    public class ProductsViewModel
    {
        public decimal TotalDailyIncome { get; set; }
        public List<InvestmentDto> Investments { get; set; } = new();
    }
}