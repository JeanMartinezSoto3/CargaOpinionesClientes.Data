using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models.Extraction;
using Microsoft.Data.SqlClient;

namespace CargaOpinionesClientes.Worker.Extractors;

public class DatabaseExtractor : IExtractor
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseExtractor> _logger;

    public DatabaseExtractor(
        IConfiguration configuration,
        ILogger<DatabaseExtractor> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<ExtractionResult>> ExtractAsync(
        CancellationToken cancellationToken)
    {
        var result = new ExtractionResult
        {
            SourceName = "BaseDatosTransaccional"
        };

        try
        {
            var connectionString =
                _configuration.GetConnectionString(
                    "TransactionalDatabase");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión TransactionalDatabase.");
            }

            var query =
                _configuration["ExtractionSettings:DatabaseQuery"];

            if (string.IsNullOrWhiteSpace(query))
            {
                query = """
                    SELECT
                        IdOpinion,
                        IdExterno,
                        TipoOpinion,
                        IdCliente,
                        IdProducto,
                        IdFuente,
                        Fecha,
                        Comentario,
                        Clasificacion,
                        PuntajeSatisfaccion,
                        Rating,
                        FechaRegistro
                    FROM dbo.Opiniones;
                    """;
            }

            await using var connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync(
                cancellationToken);

            await using var command =
                new SqlCommand(query, connection);

            command.CommandTimeout = 60;

            await using var reader =
                await command.ExecuteReaderAsync(
                    cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var data = new Dictionary<string, object?>();

                for (var index = 0;
                     index < reader.FieldCount;
                     index++)
                {
                    data[reader.GetName(index)] =
                        await reader.IsDBNullAsync(
                            index,
                            cancellationToken)
                            ? null
                            : reader.GetValue(index);
                }

                result.Records.Add(new ExtractedRecord
                {
                    Source = "dbo.Opiniones",
                    DataType = "OpinionesBaseDatos",
                    Data = data
                });
            }

            result.Success = true;

            _logger.LogInformation(
                "Datos extraídos desde la base de datos. Registros: {Total}",
                result.TotalRecords);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;

            _logger.LogError(
                ex,
                "Error durante la extracción desde la base de datos.");
        }

        return new List<ExtractionResult>
        {
            result
        };
    }
}