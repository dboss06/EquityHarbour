using EquityHarbour.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EquityHarbour.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<InvestmentPlan> InvestmentPlans { get; set; }
        public DbSet<Investment> Investments { get; set; }
        public DbSet<Deposit> Deposits { get; set; }
        public DbSet<Withdrawal> Withdrawals { get; set; }
        public DbSet<InvestmentPayout> InvestmentPayouts { get; set; }
        public DbSet<DepositAccount> DepositAccounts { get; set; }
        public DbSet<ReferralCommission> ReferralCommissions { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<GiftCode> GiftCodes { get; set; }
        public DbSet<TaskClaim> TaskClaims { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<TaskClaim>()
                .HasIndex(c => new { c.UserId, c.MilestoneTarget })
                .IsUnique();

            builder.Entity<GiftCode>()
                .HasIndex(g => g.Code)
                .IsUnique();

            builder.Entity<GiftCode>()
                .HasOne(g => g.RedeemedByUser)
                .WithMany()
                .HasForeignKey(g => g.RedeemedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<BankAccount>()
                .HasIndex(b => b.UserId)
                .IsUnique();

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.ReferredBy)
                .WithMany()
                .HasForeignKey(u => u.ReferredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.ReferralCode)
                .IsUnique();

            builder.Entity<ReferralCommission>()
                .HasOne(rc => rc.Referrer)
                .WithMany()
                .HasForeignKey(rc => rc.ReferrerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReferralCommission>()
                .HasOne(rc => rc.SourceUser)
                .WithMany()
                .HasForeignKey(rc => rc.SourceUserId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Wallet>()
                .HasOne(w => w.User)
                .WithOne(u => u.Wallet)
                .HasForeignKey<Wallet>(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Wallet>()
                .HasIndex(w => w.UserId)
                .IsUnique();

            builder.Entity<WalletTransaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Investment>()
                .HasOne(i => i.User)
                .WithMany(u => u.Investments)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Investment>()
                .HasOne(i => i.InvestmentPlan)
                .WithMany(p => p.Investments)
                .HasForeignKey(i => i.InvestmentPlanId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deposit>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Deposit>()
                .HasOne(d => d.Wallet)
                .WithMany()
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Withdrawal>()
                .HasOne(w => w.User)
                .WithMany()
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Withdrawal>()
                .HasOne(w => w.Wallet)
                .WithMany()
                .HasForeignKey(w => w.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InvestmentPayout>()
                .HasOne(p => p.Investment)
                .WithMany(i => i.Payouts)
                .HasForeignKey(p => p.InvestmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InvestmentPayout>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);

            builder.Entity<InvestmentPayout>()
                .HasIndex(p => new
                {
                    p.InvestmentId,
                    p.PeriodStart,
                    p.PeriodEnd
                })
                .IsUnique();

            builder.Entity<Withdrawal>()
                .Property(w => w.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Withdrawal>()
                .HasIndex(w => w.Reference)
                .IsUnique();

            builder.Entity<Deposit>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Deposit>()
                .HasIndex(d => d.Reference)
                .IsUnique();

            builder.Entity<WalletTransaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.AvailableBalance)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.InvestedBalance)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.TotalDeposited)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.TotalWithdrawn)
                .HasPrecision(18, 2);

            builder.Entity<Wallet>()
                .Property(w => w.TotalProfit)
                .HasPrecision(18, 2);
            builder.Entity<InvestmentPlan>()
                .Property(p => p.MinimumAmount)
                .HasPrecision(18, 2);

            builder.Entity<InvestmentPlan>()
                .Property(p => p.MaximumAmount)
                .HasPrecision(18, 2);

            builder.Entity<InvestmentPlan>()
                .Property(p => p.ReturnValue)
                .HasPrecision(18, 2);

            builder.Entity<Investment>()
                .Property(i => i.PrincipalAmount)
                .HasPrecision(18, 2);

            builder.Entity<Investment>()
                .Property(i => i.ReturnValue)
                .HasPrecision(18, 2);

            builder.Entity<Investment>()
                .Property(i => i.ExpectedReturn)
                .HasPrecision(18, 2);

            builder.Entity<InvestmentPlan>()
                .HasIndex(p => p.Name)
                .IsUnique();

            builder.Entity<Investment>()
                .HasIndex(i => i.UserId);

            builder.Entity<Investment>()
                .HasIndex(i => i.InvestmentPlanId);

            builder.Entity<Investment>()
                .HasIndex(i => i.Status);

            builder.Entity<Investment>()
                .Property(i => i.RowVersion)
                .IsRowVersion();

            builder.Entity<Wallet>()
                .Property(w => w.RowVersion)
                .IsRowVersion();
        }

    }
}
