using EquityHarbour.DTOs.GiftCodes;

namespace EquityHarbour.Services
{
    public interface IGiftCodeService
    {
        Task<GiftCodeDto> CreateAsync(CreateGiftCodeRequest request);
        Task<List<GiftCodeDto>> GetAllAsync();
        Task<decimal> RedeemAsync(string userId, string code);
    }
}