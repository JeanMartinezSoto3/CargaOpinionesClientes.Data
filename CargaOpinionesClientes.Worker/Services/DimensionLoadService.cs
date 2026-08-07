using System.Diagnostics;
using System.Globalization;
using CargaOpinionesClientes.Worker.Interfaces;
using CargaOpinionesClientes.Worker.Models.Warehouse;
using CargaOpinionesClientes.Worker.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Worker.Services;

public class DimensionLoadService : IDimensionLoadService
{
    private readonly TransactionalDbContext _transactionalContext;
    private readonly DataWarehouseDbContext _warehouseContext;
    private readonly ILogger<DimensionLoadService> _logger;

    public DimensionLoadService(
        TransactionalDbContext transactionalContext,
        DataWarehouseDbContext warehouseContext,
        ILogger<DimensionLoadService> logger)
    {
        _transactionalContext = transactionalContext;
        _warehouseContext = warehouseContext;
        _logger = logger;
    }

    public async Task LoadDimensionsAsync(
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Iniciando carga de dimensiones.");

        var clientesInsertados =
            await LoadClientesAsync(cancellationToken);

        var productosInsertados =
            await LoadProductosAsync(cancellationToken);

        var fuentesInsertadas =
            await LoadFuentesAsync(cancellationToken);

        var fechasInsertadas =
            await LoadFechasAsync(cancellationToken);

        stopwatch.Stop();

        Console.WriteLine();
        Console.WriteLine(
            "=======================================");
        Console.WriteLine(
            " RESUMEN DE CARGA DE DIMENSIONES");
        Console.WriteLine(
            "=======================================");
        Console.WriteLine(
            $"DimCliente insertados: {clientesInsertados}");
        Console.WriteLine(
            $"DimProducto insertados: {productosInsertados}");
        Console.WriteLine(
            $"DimFuente insertados: {fuentesInsertadas}");
        Console.WriteLine(
            $"DimFecha insertados: {fechasInsertadas}");
        Console.WriteLine(
            $"Tiempo total: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine(
            "=======================================");

        _logger.LogInformation(
            "Carga de dimensiones finalizada en {Time} ms.",
            stopwatch.ElapsedMilliseconds);
    }

    private async Task<int> LoadClientesAsync(
        CancellationToken cancellationToken)
    {
        var clientesOrigen =
            await _transactionalContext.Clientes
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var idsLista =
    await _warehouseContext.DimClientes
        .AsNoTracking()
        .Select(x => x.IdCliente)
        .ToListAsync(cancellationToken);

        var idsExistentes = idsLista.ToHashSet();

        var nuevosClientes = clientesOrigen
            .Where(cliente =>
                !idsExistentes.Contains(cliente.IdCliente))
            .Select(cliente => new DimCliente
            {
                IdCliente = cliente.IdCliente,
                Nombre = cliente.Nombre.Trim(),
                Email = cliente.Email.Trim(),
                Pais = null,
                TipoCliente = null
            })
            .ToList();

        if (nuevosClientes.Count == 0)
        {
            _logger.LogInformation(
                "DimCliente no tiene registros nuevos.");

            return 0;
        }

        await _warehouseContext.DimClientes.AddRangeAsync(
            nuevosClientes,
            cancellationToken);

        await _warehouseContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "DimCliente cargada. Insertados: {Count}",
            nuevosClientes.Count);

        return nuevosClientes.Count;
    }

    private async Task<int> LoadProductosAsync(
        CancellationToken cancellationToken)
    {
        var productosOrigen =
            await _transactionalContext.Productos
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var idsLista =
    await _warehouseContext.DimProductos
        .AsNoTracking()
        .Select(x => x.IdProducto)
        .ToListAsync(cancellationToken);

        var idsExistentes = idsLista.ToHashSet();

        var nuevosProductos = productosOrigen
            .Where(producto =>
                !idsExistentes.Contains(producto.IdProducto))
            .Select(producto => new DimProducto
            {
                IdProducto = producto.IdProducto,
                Nombre = producto.Nombre.Trim(),
                Categoria = producto.Categoria.Trim()
            })
            .ToList();

        if (nuevosProductos.Count == 0)
        {
            _logger.LogInformation(
                "DimProducto no tiene registros nuevos.");

            return 0;
        }

        await _warehouseContext.DimProductos.AddRangeAsync(
            nuevosProductos,
            cancellationToken);

        await _warehouseContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "DimProducto cargada. Insertados: {Count}",
            nuevosProductos.Count);

        return nuevosProductos.Count;
    }

    private async Task<int> LoadFuentesAsync(
        CancellationToken cancellationToken)
    {
        var fuentesOrigen =
            await _transactionalContext.FuentesDatos
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        var fuentesExistentes =
            await _warehouseContext.DimFuentes
                .AsNoTracking()
                .Select(x => new
                {
                    x.CodigoFuente,
                    x.NombreFuente
                })
                .ToListAsync(cancellationToken);

        var codigosExistentes = fuentesExistentes
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.CodigoFuente))
            .Select(x =>
                x.CodigoFuente!.Trim().ToLowerInvariant())
            .ToHashSet();

        var nombresExistentes = fuentesExistentes
            .Select(x =>
                x.NombreFuente.Trim().ToLowerInvariant())
            .ToHashSet();

        var nuevasFuentes = fuentesOrigen
            .Where(fuente =>
            {
                var codigo = fuente.CodigoFuente?
                    .Trim()
                    .ToLowerInvariant();

                var nombre = fuente.NombreFuente
                    .Trim()
                    .ToLowerInvariant();

                if (!string.IsNullOrWhiteSpace(codigo))
                {
                    return !codigosExistentes.Contains(codigo);
                }

                return !nombresExistentes.Contains(nombre);
            })
            .Select(fuente => new DimFuente
            {
                CodigoFuente =
                    string.IsNullOrWhiteSpace(fuente.CodigoFuente)
                        ? null
                        : fuente.CodigoFuente.Trim(),

                NombreFuente =
                    fuente.NombreFuente.Trim(),

                TipoCanal = GetTipoCanal(
                    fuente.CodigoFuente,
                    fuente.NombreFuente)
            })
            .ToList();

        if (nuevasFuentes.Count == 0)
        {
            _logger.LogInformation(
                "DimFuente no tiene registros nuevos.");

            return 0;
        }

        await _warehouseContext.DimFuentes.AddRangeAsync(
            nuevasFuentes,
            cancellationToken);

        await _warehouseContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "DimFuente cargada. Insertados: {Count}",
            nuevasFuentes.Count);

        return nuevasFuentes.Count;
    }

    private async Task<int> LoadFechasAsync(
        CancellationToken cancellationToken)
    {
        var fechasOrigen =
            await _transactionalContext.Opiniones
                .AsNoTracking()
                .Select(opinion => opinion.Fecha)
                .Distinct()
                .ToListAsync(cancellationToken);

        var idsLista =
    await _warehouseContext.DimFechas
        .AsNoTracking()
        .Select(x => x.IdFecha)
        .ToListAsync(cancellationToken);

        var idsExistentes = idsLista.ToHashSet();
        var cultura =
            CultureInfo.GetCultureInfo("es-ES");

        var nuevasFechas = fechasOrigen
            .Select(fecha => fecha.Date)
            .Distinct()
            .Select(fecha => new DimFecha
            {
                IdFecha =
                    fecha.Year * 10000 +
                    fecha.Month * 100 +
                    fecha.Day,

                Fecha = fecha,
                Dia = fecha.Day,
                Mes = fecha.Month,
                NombreMes = cultura.DateTimeFormat
                    .GetMonthName(fecha.Month),

                Trimestre =
                    ((fecha.Month - 1) / 3) + 1,

                Anio = fecha.Year
            })
            .Where(fecha =>
                !idsExistentes.Contains(fecha.IdFecha))
            .ToList();

        if (nuevasFechas.Count == 0)
        {
            _logger.LogInformation(
                "DimFecha no tiene registros nuevos.");

            return 0;
        }

        await _warehouseContext.DimFechas.AddRangeAsync(
            nuevasFechas,
            cancellationToken);

        await _warehouseContext.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "DimFecha cargada. Insertados: {Count}",
            nuevasFechas.Count);

        return nuevasFechas.Count;
    }

    private static string GetTipoCanal(
        string? codigoFuente,
        string nombreFuente)
    {
        var texto =
            $"{codigoFuente} {nombreFuente}"
                .ToLowerInvariant();

        if (texto.Contains("web") ||
            texto.Contains("review") ||
            texto.Contains("reseña"))
        {
            return "Sitio web";
        }

        if (texto.Contains("social") ||
            texto.Contains("facebook") ||
            texto.Contains("instagram") ||
            texto.Contains("twitter"))
        {
            return "Red social";
        }

        if (texto.Contains("survey") ||
            texto.Contains("encuesta"))
        {
            return "Encuesta";
        }

        if (texto.Contains("api"))
        {
            return "API REST";
        }

        return "No especificado";
    }
}