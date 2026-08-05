using CargaOpinionesClientes.Worker.Services;

namespace CargaOpinionesClientes.Worker;

public class Worker : BackgroundService
{
    private readonly ExtractionOrchestratorService
        _orchestrator;

    private readonly ILogger<Worker> _logger;

    private readonly IHostApplicationLifetime
        _applicationLifetime;

    public Worker(
        ExtractionOrchestratorService orchestrator,
        ILogger<Worker> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "=======================================");

        Console.WriteLine(
            " WORKER SERVICE - EXTRACCION ETL");

        Console.WriteLine(
            "=======================================");

        _logger.LogInformation(
            "Worker Service iniciado.");

        try
        {
            await _orchestrator.ExecuteAsync(
                stoppingToken);

            _logger.LogInformation(
                "Proceso de extracción finalizado correctamente.");

            Console.WriteLine();
            Console.WriteLine(
                "Proceso de extracción finalizado correctamente.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "El proceso fue cancelado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error general durante el proceso de extracción.");

            Console.WriteLine(
                $"Error general: {ex.Message}");
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}