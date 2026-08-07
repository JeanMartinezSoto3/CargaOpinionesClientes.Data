using System.Diagnostics;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models.Extraction;

namespace CargaOpinionesClientes.Worker.Services;

public class ExtractionOrchestratorService
{
    private readonly IEnumerable<IExtractor> _extractors;
    private readonly StagingWriterService _stagingWriter;
    private readonly ILogger<ExtractionOrchestratorService>
        _logger;

    public ExtractionOrchestratorService(
        IEnumerable<IExtractor> extractors,
        StagingWriterService stagingWriter,
        ILogger<ExtractionOrchestratorService> logger)
    {
        _extractors = extractors;
        _stagingWriter = stagingWriter;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        var extractors = _extractors.ToList();

        _logger.LogInformation(
            "Iniciando proceso de extracción.");

        Console.WriteLine(
            $"Cantidad de extractores registrados: {extractors.Count}");

        var extractionTasks = extractors.Select(
            extractor => ExecuteExtractorAsync(
                extractor,
                cancellationToken));

        var resultsByExtractor =
            await Task.WhenAll(extractionTasks);

        var allResults = resultsByExtractor
            .SelectMany(results => results)
            .ToList();

        await _stagingWriter.WriteAsync(
            allResults,
            cancellationToken);

        stopwatch.Stop();

        ShowSummary(
            allResults,
            stopwatch.ElapsedMilliseconds);

        _logger.LogInformation(
            "Proceso de extracción finalizado en {Time} ms.",
            stopwatch.ElapsedMilliseconds);
    }

    private async Task<List<ExtractionResult>>
        ExecuteExtractorAsync(
            IExtractor extractor,
            CancellationToken cancellationToken)
    {
        var extractorName =
            extractor.GetType().Name;

        Console.WriteLine(
            $"Ejecutando extractor: {extractorName}");

        try
        {
            var results =
                await extractor.ExtractAsync(
                    cancellationToken);

            _logger.LogInformation(
                "Extractor {Extractor} finalizado.",
                extractorName);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error no controlado en el extractor {Extractor}.",
                extractorName);

            return new List<ExtractionResult>
            {
                new()
                {
                    SourceName = extractorName,
                    Success = false,
                    ErrorMessage = ex.Message
                }
            };
        }
    }

    private static void ShowSummary(
        IReadOnlyCollection<ExtractionResult> results,
        long elapsedMilliseconds)
    {
        var successfulSources =
            results.Count(result => result.Success);

        var failedSources =
            results.Count(result => !result.Success);

        var totalRecords = results
            .Where(result => result.Success)
            .Sum(result => result.TotalRecords);

        Console.WriteLine();
        Console.WriteLine(
            "=======================================");
        Console.WriteLine(
            " RESUMEN DEL PROCESO DE EXTRACCION");
        Console.WriteLine(
            "=======================================");

        Console.WriteLine(
            $"Fuentes procesadas: {results.Count}");

        Console.WriteLine(
            $"Fuentes exitosas: {successfulSources}");

        Console.WriteLine(
            $"Fuentes con errores: {failedSources}");

        Console.WriteLine(
            $"Registros extraídos: {totalRecords}");

        Console.WriteLine(
            $"Tiempo total: {elapsedMilliseconds} ms");

        Console.WriteLine(
            "=======================================");
    }
}