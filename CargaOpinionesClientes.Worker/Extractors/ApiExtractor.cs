using System.Text.Json;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models;

namespace CargaOpinionesClientes.Worker.Extractors;

public class ApiExtractor : IExtractor
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ApiExtractor> _logger;

    public ApiExtractor(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ApiExtractor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<ExtractionResult>> ExtractAsync(
        CancellationToken cancellationToken)
    {
        var result = new ExtractionResult
        {
            SourceName = "ApiComentarios"
        };

        try
        {
            var url =
                _configuration["ApiSettings:CommentsUrl"];

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException(
                    "No se encontró ApiSettings:CommentsUrl.");
            }

            var client =
                _httpClientFactory.CreateClient("CommentsApi");

            using var response =
                await client.GetAsync(
                    url,
                    cancellationToken);

            response.EnsureSuccessStatusCode();

            var json =
                await response.Content.ReadAsStringAsync(
                    cancellationToken);

            using var document =
                JsonDocument.Parse(json);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Array)
            {
                throw new JsonException(
                    "La respuesta de la API no contiene una colección.");
            }

            foreach (var item in
                     document.RootElement.EnumerateArray())
            {
                var data = ConvertJsonObject(item);

                result.Records.Add(new ExtractedRecord
                {
                    Source = url,
                    DataType = "ComentariosApi",
                    Data = data
                });
            }

            result.Success = true;

            _logger.LogInformation(
                "Datos extraídos desde la API REST. Registros: {Total}",
                result.TotalRecords);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;

            _logger.LogError(
                ex,
                "Error durante la extracción desde la API REST.");
        }

        return new List<ExtractionResult>
        {
            result
        };
    }

    private static Dictionary<string, object?>
        ConvertJsonObject(JsonElement element)
    {
        var data = new Dictionary<string, object?>();

        foreach (var property in
                 element.EnumerateObject())
        {
            data[property.Name] =
                ConvertJsonValue(property.Value);
        }

        return data;
    }

    private static object? ConvertJsonValue(
        JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String =>
                element.GetString(),

            JsonValueKind.Number
                when element.TryGetInt64(out var integer) =>
                integer,

            JsonValueKind.Number
                when element.TryGetDecimal(out var decimalValue) =>
                decimalValue,

            JsonValueKind.True => true,

            JsonValueKind.False => false,

            JsonValueKind.Null => null,

            _ => element.GetRawText()
        };
    }
}