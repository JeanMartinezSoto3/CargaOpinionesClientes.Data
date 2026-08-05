using System.Globalization;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CargaOpinionesClientes.Worker.Extractors;

public class CsvExtractor : IExtractor
{
    private readonly ILogger<CsvExtractor> _logger;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public CsvExtractor(
        ILogger<CsvExtractor> logger,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _logger = logger;
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<List<ExtractionResult>> ExtractAsync(
        CancellationToken cancellationToken)
    {
        var results = new List<ExtractionResult>();

        var configuredFolder =
            _configuration["ExtractionSettings:CsvFolder"] ?? "Data";

        var dataFolder = Path.Combine(
            _environment.ContentRootPath,
            configuredFolder);

        if (!Directory.Exists(dataFolder))
        {
            _logger.LogWarning(
                "La carpeta de archivos CSV no existe: {Folder}",
                dataFolder);

            return results;
        }

        var csvFiles = Directory.GetFiles(
            dataFolder,
            "*.csv",
            SearchOption.TopDirectoryOnly);

        foreach (var filePath in csvFiles)
        {
            var result = new ExtractionResult
            {
                SourceName = Path.GetFileName(filePath)
            };

            try
            {
                var records = await ReadCsvAsync(
                    filePath,
                    cancellationToken);

                result.Success = true;
                result.Records = records;

                _logger.LogInformation(
                    "Archivo CSV extraído: {File}. Registros: {Total}",
                    result.SourceName,
                    result.TotalRecords);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;

                _logger.LogError(
                    ex,
                    "Error al extraer el archivo CSV {File}",
                    result.SourceName);
            }

            results.Add(result);
        }

        return results;
    }

    private async Task<List<ExtractedRecord>> ReadCsvAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        var records = new List<ExtractedRecord>();

        var configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim
        };

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        using var reader = new StreamReader(stream);

        using var csv = new CsvReader(
            reader,
            configuration);

        await foreach (
            var dynamicRecord in csv
                .GetRecordsAsync<dynamic>(cancellationToken))
        {
            var sourceDictionary =
                (IDictionary<string, object>)dynamicRecord;

            var data = sourceDictionary.ToDictionary(
                item => item.Key,
                item => (object?)item.Value);

            records.Add(new ExtractedRecord
            {
                Source = Path.GetFileName(filePath),
                DataType = GetDataType(filePath),
                Data = data
            });
        }

        return records;
    }

    private static string GetDataType(string filePath)
    {
        var fileName = Path
            .GetFileNameWithoutExtension(filePath)
            .ToLowerInvariant();

        return fileName switch
        {
            "clients" => "Clientes",
            "products" => "Productos",
            "fuente_datos" => "FuentesDatos",
            "social_comments" => "ComentariosSociales",
            "surveys_part1" => "Encuestas",
            "web_reviews" => "ReseñasWeb",
            _ => "Desconocido"
        };
    }
}