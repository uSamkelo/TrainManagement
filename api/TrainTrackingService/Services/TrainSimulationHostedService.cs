using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class TrainSimulationHostedService : BackgroundService
{
    private readonly TrainSimulationService _trainSimulationService;
    private readonly ILogger<TrainSimulationHostedService> _logger;

    public TrainSimulationHostedService(TrainSimulationService trainSimulationService, ILogger<TrainSimulationHostedService> logger)
    {
        _trainSimulationService = trainSimulationService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Train Simulation Hosted Service starting");
        try
        {
            // The main simulation loop runs in TrainSimulationService.StartAsync()
            await _trainSimulationService.StartAsync();
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Train Simulation Hosted Service cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Train Simulation Hosted Service failed");
        }
    }
}
