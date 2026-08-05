using System.Text.Json;
using CargaOpinionesClientes.Worker.Models;

namespace CargaOpinionesClientes.Worker.Services;

public class StagingWriterService
{
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StagingWriterService> _logger;

    public StagingWriterService(
        IHostEnvironment environment,
        IConfiguration configuration,
        ILogger<StagingWriterService> logger)
    {
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task WriteAsync(
        IEnumerable<ExtractionResult> results,
        CancellationToken cancellationToken)
    {
        var temporaryFolder =
            _configuration[
                "ExtractionSettings:TemporaryFolder"]
            ?? "TemporaryData";

        var outputFolder = Path.Combine(
            _environment.ContentRootPath,
            temporaryFolder);

        Directory.CreateDirectory(outputFolder);

        var successfulResults = results
            .Where(result => result.Success)
            .ToList();

        foreach (var result in successfulResults)
        {
            var safeName = CreateSafeFileName(
                result.SourceName);

            var timestamp =
                DateTime.Now.ToString("yyyyMMdd_HHmmss");

            var fileName =
                $"{safeName}_{timestamp}.json";

            var outputPath = Path.Combine(
                outputFolder,
                fileName);

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            await using var stream =
                new FileStream(
                    outputPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 4096,
                    useAsync: true);

            await JsonSerializer.SerializeAsync(
                stream,
                result.Records,
                options,
                cancellationToken);

            _logger.LogInformation(
                "Datos temporales almacenados: {Path}. Registros: {Total}",
                outputPath,
                result.TotalRecords);
        }
    }

    private static string CreateSafeFileName(
        string sourceName)
    {
        var invalidCharacters =
            Path.GetInvalidFileNameChars();

        var cleanName = new string(
            sourceName
                .Select(character =>
                    invalidCharacters.Contains(character)
                        ? '_'
                        : character)
                .ToArray());

        return Path.GetFileNameWithoutExtension(
            cleanName);
    }
}