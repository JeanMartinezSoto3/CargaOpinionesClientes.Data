using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Services;
using Microsoft.Extensions.Configuration;

Console.WriteLine("=====================================");
Console.WriteLine(" ETL - Análisis de Opiniones Clientes ");
Console.WriteLine("=====================================");
Console.WriteLine();

IConfiguration configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

string? connectionString = configuration.GetConnectionString("ClienteOpinionesDB");

if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.WriteLine("No se encontró la cadena de conexión en appsettings.json.");
    Console.ReadKey();
    return;
}

string carpetaData = Path.Combine(AppContext.BaseDirectory, "Data");

if (!Directory.Exists(carpetaData))
{
    Console.WriteLine("No se encontró la carpeta Data.");
    Console.WriteLine($"Ruta buscada: {carpetaData}");
    Console.ReadKey();
    return;
}

try
{
    var context = new OpinionesDbContext(connectionString);

    var errorService = new ErrorCargaService(context);
    var clienteService = new ClienteService(context, errorService);
    var productoService = new ProductoService(context, errorService);
    var fuenteDatoService = new FuenteDatoService(context, errorService);
    var opinionService = new OpinionService(context, fuenteDatoService, errorService);

    var etlProcesoService = new EtlProcesoService(
        clienteService,
        productoService,
        fuenteDatoService,
        opinionService
    );

    etlProcesoService.Ejecutar(carpetaData);
}
catch (Exception ex)
{
    Console.WriteLine("Error general del proceso ETL:");
    Console.WriteLine(ex.Message);
}

Console.WriteLine();
Console.WriteLine("Presiona una tecla para salir...");
Console.ReadKey();