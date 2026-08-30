using EquityHarbour.Data;
using EquityHarbour.DTOs.DepositAccounts;
using EquityHarbour.Models;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Services
{
    public class DepositAccountService : IDepositAccountService
    {
        private readonly ApplicationDbContext _context;

        public DepositAccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepositAccountDto>> GetAllAsync()
        {
            return await _context.DepositAccounts
                .AsNoTracking()
                .OrderBy(a => a.DisplayOrder)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<List<DepositAccountDto>> GetActiveAsync()
        {
            return await _context.DepositAccounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.DisplayOrder)
                .Select(a => MapToDto(a))
                .ToListAsync();
        }

        public async Task<DepositAccountDto> CreateAsync(CreateDepositAccountRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.BankName))
                throw new ArgumentException("Bank name is required.");
            if (string.IsNullOrWhiteSpace(request.AccountNumber))
                throw new ArgumentException("Account number is required.");
            if (string.IsNullOrWhiteSpace(request.AccountName))
                throw new ArgumentException("Account name is required.");

            var account = new DepositAccount
            {
                BankName = request.BankName.Trim(),
                AccountNumber = request.AccountNumber.Trim(),
                AccountName = request.AccountName.Trim(),
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.DepositAccounts.Add(account);
            await _context.SaveChangesAsync();

            return MapToDto(account);
        }

        public async Task<bool> UpdateAsync(int id, CreateDepositAccountRequest request)
        {
            var account = await _context.DepositAccounts.FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return false;

            account.BankName = request.BankName.Trim();
            account.AccountNumber = request.AccountNumber.Trim();
            account.AccountName = request.AccountName.Trim();
            account.DisplayOrder = request.DisplayOrder;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetActiveAsync(int id, bool isActive)
        {
            var account = await _context.DepositAccounts.FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return false;

            account.IsActive = isActive;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _context.DepositAccounts.FirstOrDefaultAsync(a => a.Id == id);
            if (account == null) return false;

            _context.DepositAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return true;
        }

        private static DepositAccountDto MapToDto(DepositAccount a) => new()
        {
            Id = a.Id,
            BankName = a.BankName,
            AccountNumber = a.AccountNumber,
            AccountName = a.AccountName,
            IsActive = a.IsActive,
            DisplayOrder = a.DisplayOrder
        };
    }
}