using PharmacyStock.Application.Interfaces;
using PharmacyStock.Application.Services;

namespace PharmacyStock.API.Services;

/// <summary>
/// Background service that runs scheduled tasks for batch status updates and notification generation
/// Runs daily at 6:00 AM
/// </summary>
public class ScheduledBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScheduledBackgroundService> _logger;
    private readonly TimeSpan _scheduledTime = new TimeSpan(6, 0, 0); // 6:00 AM
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(60); // Check every 60 minutes

    public ScheduledBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<ScheduledBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Scheduled Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Check if we've already run today using cache for persistence
                using var scope = _serviceProvider.CreateScope();
                var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();
                var cacheKey = "PharmacyStock_DailyJob_LastRun";
                var today = DateTime.Today.ToString("yyyy-MM-dd");

                var lastRun = await cache.GetAsync<string>(cacheKey);
                var hasRunToday = lastRun == today;

                // Run if we haven't run today AND it is past the scheduled time
                if (!hasRunToday && DateTime.Now.TimeOfDay >= _scheduledTime)
                {
                    _logger.LogInformation("Running scheduled tasks at {Time} (Scheduled: {ScheduledTime})", DateTime.Now, _scheduledTime);
                    await RunScheduledTasksAsync(stoppingToken);

                    // Mark as run for today in cache (expire in 26 hours)
                    await cache.SetAsync(cacheKey, today, TimeSpan.FromHours(26));
                }

                // Wait for the next check interval
                await Task.Delay(_checkInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in scheduled background service");
                // Continue running even if there's an error (wait before retrying)
                try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); } catch { }
            }
        }

        _logger.LogInformation("Scheduled Background Service stopped");
    }

    private async Task RunScheduledTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            // Task 1: Update batch statuses
            _logger.LogInformation("Starting batch status update task");
            var batchStatusService = scope.ServiceProvider.GetRequiredService<BatchStatusUpdateService>();
            await batchStatusService.UpdateAllBatchStatusesAsync(cancellationToken);
            _logger.LogInformation("Completed batch status update task");

            // Task 2: Generate notifications
            _logger.LogInformation("Starting notification generation task");
            var notificationGenerator = scope.ServiceProvider.GetRequiredService<INotificationGeneratorService>();
            await notificationGenerator.GenerateAllNotificationsAsync(cancellationToken);
            _logger.LogInformation("Completed notification generation task");

            _logger.LogInformation("All scheduled tasks completed successfully");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Scheduled tasks cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running scheduled tasks");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduled Background Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
