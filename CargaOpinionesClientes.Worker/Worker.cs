using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Services;

namespace CargaOpinionesClientes.Worker;

public class Worker : BackgroundService
{
    private readonly ExtractionOrchestratorService _orchestrator;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public Worker(
        ExtractionOrchestratorService orchestrator,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger,
        IHostApplicationLifetime applicationLifetime)
    {
        _orchestrator = orchestrator;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        Console.WriteLine(
            "=======================================");

        Console.WriteLine(
            " WORKER SERVICE - PROCESO ETL");

        Console.WriteLine(
            "=======================================");

        _logger.LogInformation(
            "Worker Service iniciado.");

        try
        {
            // =============================================
            // 1. EXTRACCIÓN
            // =============================================

            Console.WriteLine();
            Console.WriteLine(
                "INICIANDO ETAPA DE EXTRACCION...");
            Console.WriteLine();

            await _orchestrator.ExecuteAsync(
                stoppingToken);

            _logger.LogInformation(
                "Proceso de extracción finalizado correctamente.");

            // =============================================
            // 2. CARGA DE DIMENSIONES
            // =============================================

            Console.WriteLine();
            Console.WriteLine(
                "INICIANDO CARGA DE DIMENSIONES...");
            Console.WriteLine();

            _logger.LogInformation(
                "Iniciando carga de dimensiones.");

            using var scope =
                _scopeFactory.CreateScope();

            var dimensionLoadService =
                scope.ServiceProvider
                    .GetRequiredService<IDimensionLoadService>();

            await dimensionLoadService.LoadDimensionsAsync(
                stoppingToken);

            // =============================================
            // FIN
            // =============================================

            _logger.LogInformation(
                "Proceso ETL finalizado correctamente.");

            Console.WriteLine();
            Console.WriteLine(
                "=======================================");

            Console.WriteLine(
                " PROCESO FINALIZADO CORRECTAMENTE");

            Console.WriteLine(
                "=======================================");

            Console.WriteLine(
                "Extracción y carga de dimensiones completadas.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "El proceso fue cancelado.");

            Console.WriteLine(
                "El proceso fue cancelado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error general durante el proceso ETL.");

            Console.WriteLine();
            Console.WriteLine(
                "ERROR DURANTE EL PROCESO:");

            Console.WriteLine(
                ex.Message);
        }
        finally
        {
            _applicationLifetime.StopApplication();
        }
    }
}