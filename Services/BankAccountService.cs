using EquityHarbour.Data;
using EquityHarbour.DTOs.BankAccounts;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class BankAccountService : IBankAccountService
    {
        private readonly ApplicationDbContext _context;

        public BankAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BankAccountDto?> GetByUserIdAsync(string userId)
        {
            return await _context.BankAccounts
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .Select(b => MapToDto(b))
                .FirstOrDefaultAsync();
        }

        public async Task<BankAccountDto> UpsertAsync(string userId, SaveBankAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BankName))
                throw new ArgumentException("Bank name is required.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Account number is required.");
            if (string.IsNullOrWhiteSpace(request.AccountName))
                throw new ArgumentException("Account name is required.");

            var account = await _context.BankAccounts.FirstOrDefaultAsync(b => b.UserId == userId);

            if (account == null)
            {
                account = new BankAccount
                {
                    UserId = userId,
                    BankName = request.BankName.Trim(),
                    AccountNumber = request.AccountNumber.Trim(),
                    AccountName = request.AccountName.Trim(),
                    CreatedAt = DateTime.UtcNow
                };
                _context.BankAccounts.Add(account);
            }
            else
            {
                account.BankName = request.BankName.Trim();
                account.AccountNumber = request.AccountNumber.Trim();
                account.AccountName = request.AccountName.Trim();
                account.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(account);
        }

        private static BankAccountDto MapToDto(BankAccount b) => new()
        {
            Id = b.Id,
            BankName = b.BankName,
            AccountNumber = b.AccountNumber,
            AccountName = b.AccountName,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };
    }
}