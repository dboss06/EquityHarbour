using EquityHarbour.Data;
using EquityHarbour.DTOs.GiftCodes;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class GiftCodeService : IGiftCodeService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWalletService _walletService;

        public GiftCodeService(ApplicationDbContext context, IWalletService walletService)
        {
            _context = context;
            _walletService = walletService;
        }

        public async Task<GiftCodeDto> CreateAsync(CreateGiftCodeRequest request)
        {
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Gift code amount must be greater than zero.");
            }

            string code;
            do
            {
                code = Guid.NewGuid().ToString("N")[..10].ToUpperInvariant();
            } while (await _context.GiftCodes.AnyAsync(g => g.Code == code));

            var giftCode = new GiftCode
            {
                Code = code,
                Amount = request.Amount,
                IsRedeemed = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.GiftCodes.Add(giftCode);
            await _context.SaveChangesAsync();

            return MapToDto(giftCode);
        }

        public async Task<List<GiftCodeDto>> GetAllAsync()
        {
            return await _context.GiftCodes
                .AsNoTracking()
                .Include(g => g.RedeemedByUser)
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new GiftCodeDto
                {
                    Id = g.Id,
                    Code = g.Code,
                    Amount = g.Amount,
                    IsRedeemed = g.IsRedeemed,
                    RedeemedByUserFullName = g.RedeemedByUser != null ? g.RedeemedByUser.FullName : null,
                    RedeemedAt = g.RedeemedAt,
                    CreatedAt = g.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<decimal> RedeemAsync(string userId, string code)
        {
            var giftCode = await _context.GiftCodes.FirstOrDefaultAsync(g => g.Code == code.Trim().ToUpperInvariant());
            if (giftCode == null)
            {
                throw new InvalidOperationException("Invalid gift code.");
            }
            if (giftCode.IsRedeemed)
            {
                throw new InvalidOperationException("This gift code has already been redeemed.");
            }

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
            {
                throw new InvalidOperationException("Wallet not found.");
            }

            giftCode.IsRedeemed = true;
            giftCode.RedeemedByUserId = userId;
            giftCode.RedeemedAt = DateTime.UtcNow;

            await _walletService.CreditAsync(
                wallet.Id,
                giftCode.Amount,
                WalletTransactionType.GiftCode,
                "Gift code redeemed",
                $"GIFT-{giftCode.Code}");

            await _context.SaveChangesAsync();

            return giftCode.Amount;
        }

        private static GiftCodeDto MapToDto(GiftCode g) => new()
        {
            Id = g.Id,
            Code = g.Code,
            Amount = g.Amount,
            IsRedeemed = g.IsRedeemed,
            RedeemedByUserFullName = g.RedeemedByUser?.FullName,
            RedeemedAt = g.RedeemedAt,
            CreatedAt = g.CreatedAt
        };
    }
}