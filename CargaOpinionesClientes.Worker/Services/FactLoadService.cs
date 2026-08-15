using System.Diagnostics;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models.Warehouse;
using CargaOpinionesClientes.Worker.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Worker.Services;

public class FactLoadService : IFactLoadService
{
    private readonly TransactionalDbContext _transactionalContext;
    private readonly DataWarehouseDbContext _warehouseContext;
    private readonly ILogger<FactLoadService> _logger;

    public FactLoadService(
        TransactionalDbContext transactionalContext,
        DataWarehouseDbContext warehouseContext,
        ILogger<FactLoadService> logger)
    {
        _transactionalContext = transactionalContext;
        _warehouseContext = warehouseContext;
        _logger = logger;
    }

    public async Task LoadFactsAsync(
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando carga de FactOpiniones.");

        var opinionesOrigen =
            await _transactionalContext.Opiniones
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var fuentesOrigen =
            await _transactionalContext.FuentesDatos
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var fuentesDW =
            await _warehouseContext.DimFuentes
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var idsExternosExistentes =
            await _warehouseContext.FactOpiniones
                .AsNoTracking()
                .Select(x => x.IdExterno)
                .ToListAsync(cancellationToken);

        var idsExternosSet =
            idsExternosExistentes
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var factsNuevas = new List<FactOpinion>();

        foreach (var opinion in opinionesOrigen)
        {
            if (idsExternosSet.Contains(opinion.IdExterno))
            {
                continue;
            }

            var fuenteOrigen = fuentesOrigen
                .FirstOrDefault(x =>
                    x.IdFuente == opinion.IdFuente);

            if (fuenteOrigen is null)
            {
                _logger.LogWarning(
                    "No se encontró fuente origen para la opinión {IdExterno}.",
                    opinion.IdExterno);

                continue;
            }

            var fuenteDW = fuentesDW
                .FirstOrDefault(x =>
                    x.CodigoFuente == fuenteOrigen.CodigoFuente);

            if (fuenteDW is null)
            {
                _logger.LogWarning(
                    "No se encontró DimFuente para la opinión {IdExterno}.",
                    opinion.IdExterno);

                continue;
            }

            var idFecha =
                opinion.Fecha.Year * 10000 +
                opinion.Fecha.Month * 100 +
                opinion.Fecha.Day;

            var clienteExiste =
                await _warehouseContext.DimClientes
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.IdCliente == opinion.IdCliente,
                        cancellationToken);

            var productoExiste =
                await _warehouseContext.DimProductos
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.IdProducto == opinion.IdProducto,
                        cancellationToken);

            var fechaExiste =
                await _warehouseContext.DimFechas
                    .AsNoTracking()
                    .AnyAsync(
                        x => x.IdFecha == idFecha,
                        cancellationToken);

            if (!clienteExiste ||
                !productoExiste ||
                !fechaExiste)
            {
                _logger.LogWarning(
                    "La opinión {IdExterno} no pudo cargarse porque falta una dimensión relacionada.",
                    opinion.IdExterno);

                continue;
            }

            factsNuevas.Add(
                new FactOpinion
                {
                    IdExterno = opinion.IdExterno.Trim(),
                    TipoOpinion = opinion.TipoOpinion.Trim(),
                    IdCliente = opinion.IdCliente,
                    IdProducto = opinion.IdProducto,
                    IdFuente = fuenteDW.IdFuente,
                    IdFecha = idFecha,
                    Comentario = opinion.Comentario.Trim(),
                    Clasificacion = opinion.Clasificacion?.Trim(),
                    PuntajeSatisfaccion =
                        opinion.PuntajeSatisfaccion,
                    Rating = opinion.Rating,
                    FechaRegistro = opinion.FechaRegistro
                });
        }

        if (factsNuevas.Count > 0)
        {
            await _warehouseContext.FactOpiniones
                .AddRangeAsync(
                    factsNuevas,
                    cancellationToken);

            await _warehouseContext
                .SaveChangesAsync(cancellationToken);
        }

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine(
            "=======================================");
        Console.WriteLine(
            " RESUMEN DE CARGA DE FACT");
        Console.WriteLine(
            "=======================================");
        Console.WriteLine(
            $"FactOpiniones insertadas: {factsNuevas.Count}");
        Console.WriteLine(
            $"Tiempo total: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine(
            "=======================================");

        _logger.LogInformation(
            "Carga de FactOpiniones finalizada. Insertados: {Count}",
            factsNuevas.Count);
    }
}