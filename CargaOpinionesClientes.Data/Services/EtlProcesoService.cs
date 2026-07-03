using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Result;

namespace CargaOpinionesClientes.Data.Services
{
    public class EtlProcesoService
    {
        private readonly IClienteService _clienteService;
        private readonly IProductoService _productoService;
        private readonly IFuenteDatoService _fuenteDatoService;
        private readonly IOpinionService _opinionService;

        public EtlProcesoService(
            IClienteService clienteService,
            IProductoService productoService,
            IFuenteDatoService fuenteDatoService,
            IOpinionService opinionService)
        {
            _clienteService = clienteService;
            _productoService = productoService;
            _fuenteDatoService = fuenteDatoService;
            _opinionService = opinionService;
        }

        public void Ejecutar(string carpetaData)
        {
            Console.WriteLine("Iniciando proceso ETL...");
            Console.WriteLine();

            var resultados = new List<ResultadoCarga>
            {
                _clienteService.CargarClientes(Path.Combine(carpetaData, "clients.csv")),
                _productoService.CargarProductos(Path.Combine(carpetaData, "products.csv")),
                _fuenteDatoService.CargarFuentes(Path.Combine(carpetaData, "fuente_datos.csv")),
                _opinionService.CargarEncuestas(Path.Combine(carpetaData, "surveys_part1.csv")),
                _opinionService.CargarWebReviews(Path.Combine(carpetaData, "web_reviews.csv")),
                _opinionService.CargarSocialComments(Path.Combine(carpetaData, "social_comments.csv"))
            };

            Console.WriteLine();
            Console.WriteLine("=========== RESUMEN FINAL ===========");

            foreach (var resultado in resultados)
            {
                resultado.Mostrar();
            }

            Console.WriteLine("Proceso ETL finalizado.");
        }
    }
}