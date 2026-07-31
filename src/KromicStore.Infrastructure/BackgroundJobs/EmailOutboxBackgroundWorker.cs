using KromicStore.Infrastructure.Services.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KromicStore.Infrastructure.BackgroundJobs;

/// <summary>
/// Background worker for processing email outbox.
/// Runs periodically to process pending emails and retries.
/// </summary>
public class EmailOutboxBackgroundWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailOutboxBackgroundWorker> _logger;
    private readonly int _processingIntervalSeconds = 30;
    private readonly int _retryIntervalSeconds = 60;

    public EmailOutboxBackgroundWorker(
        IServiceProvider serviceProvider,
        ILogger<EmailOutboxBackgroundWorker> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailOutboxBackgroundWorker started");

        try
        {
            // Start processing task
            var processingTask = ProcessPendingEmailsAsync(stoppingToken);
            
            // Start retry task
            var retryTask = ProcessRetryEmailsAsync(stoppingToken);

            await Task.WhenAll(processingTask, retryTask);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("EmailOutboxBackgroundWorker is stopping");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in EmailOutboxBackgroundWorker");
            throw;
        }
        finally
        {
            _logger.LogInformation("EmailOutboxBackgroundWorker stopped");
        }
    }

    private async Task ProcessPendingEmailsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<EmailOutboxProcessor>();

                var processedCount = await processor.ProcessPendingAsync(50, stoppingToken);
                if (processedCount > 0)
                {
                    _logger.LogInformation("Processed {Count} pending emails", processedCount);
                }

                await Task.Delay(TimeSpan.FromSeconds(_processingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending emails");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task ProcessRetryEmailsAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<EmailOutboxProcessor>();

                var processedCount = await processor.ProcessRetriesAsync(50, stoppingToken);
                if (processedCount > 0)
                {
                    _logger.LogInformation("Processed {Count} retry emails", processedCount);
                }

                await Task.Delay(TimeSpan.FromSeconds(_retryIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing retry emails");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
