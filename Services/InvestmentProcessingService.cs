using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EquityHarbour.Services
{

    public class InvestmentProcessingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<InvestmentProcessingService> _logger;

        // For development we'll check every minute.
        private static readonly TimeSpan Interval =
            TimeSpan.FromMinutes(1);

        public InvestmentProcessingService(
            IServiceScopeFactory scopeFactory,
            ILogger<InvestmentProcessingService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Investment processing service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessInvestmentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred while processing investments.");
                }

                try
                {
                    await Task.Delay(
                        Interval,
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation(
                "Investment processing service stopped.");
        }

        private async Task ProcessInvestmentsAsync()
        {
            using var scope =
                _scopeFactory.CreateScope();

            var payoutService =
                scope.ServiceProvider
                    .GetRequiredService<IInvestmentPayoutService>();

            var investmentService =
                scope.ServiceProvider
                    .GetRequiredService<IInvestmentService>();

            // Process daily/weekly/monthly payouts.
            var payoutsProcessed =
                await payoutService
                    .ProcessAllPendingPayoutsAsync();

            if (payoutsProcessed > 0)
            {
                _logger.LogInformation(
                    "Processed {Count} investment payouts.",
                    payoutsProcessed);
            }

            // Unlock any previously-locked payouts that are now due.
            var unlockedCount = await payoutService.UnlockDuePayoutsAsync();
            if (unlockedCount > 0)
            {
                _logger.LogInformation("Unlocked {Count} previously-locked investment payouts.", unlockedCount);
            }

            // Process matured investments.
            var maturedInvestments =
                await investmentService
                    .ProcessMaturedInvestmentsAsync();

            if (maturedInvestments.Count > 0)
            {
                _logger.LogInformation(
                    "Processed {Count} matured investments.",
                    maturedInvestments.Count);
            }
        }
    }
}