using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Helpers;
using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Models;
using CargaOpinionesClientes.Data.Result;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Data.Services
{
    public class OpinionService : IOpinionService
    {
        private readonly OpinionesDbContext _context;
        private readonly IFuenteDatoService _fuenteDatoService;
        private readonly IErrorCargaService _errorService;

        public OpinionService(
            OpinionesDbContext context,
            IFuenteDatoService fuenteDatoService,
            IErrorCargaService errorService)
        {
            _context = context;
            _fuenteDatoService = fuenteDatoService;
            _errorService = errorService;
        }

        public ResultadoCarga CargarEncuestas(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "surveys_part1.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var clientes = _context.Clientes.AsNoTracking().Select(c => c.IdCliente).ToHashSet();
                var productos = _context.Productos.AsNoTracking().Select(p => p.IdProducto).ToHashSet();
                var fuente = _fuenteDatoService.ObtenerOCrearFuente("EncuestaInterna");

                var duplicados = _context.Opiniones
                    .AsNoTracking()
                    .Where(o => o.TipoOpinion == "ENCUESTA")
                    .Select(o => o.IdExterno)
                    .ToHashSet();

                var opiniones = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdOpinion", "IdCliente", "IdProducto", "Fecha", "Comentario", "Clasificación", "PuntajeSatisfacción", "Fuente"))
                    .Select(f => new
                    {
                        IdExterno = TransformacionHelper.LimpiarTexto(f["IdOpinion"]),
                        IdClienteValido = int.TryParse(f["IdCliente"], out int _),
                        IdCliente = int.TryParse(f["IdCliente"], out int idCliente) ? idCliente : 0,
                        IdProductoValido = int.TryParse(f["IdProducto"], out int _),
                        IdProducto = int.TryParse(f["IdProducto"], out int idProducto) ? idProducto : 0,
                        FechaValida = DateTime.TryParse(f["Fecha"], out DateTime _),
                        Fecha = DateTime.TryParse(f["Fecha"], out DateTime fecha) ? fecha : DateTime.MinValue,
                        Comentario = TransformacionHelper.LimpiarTexto(f["Comentario"]),
                        Clasificacion = TransformacionHelper.LimpiarTexto(f["Clasificación"]),
                        PuntajeValido = int.TryParse(f["PuntajeSatisfacción"], out int _),
                        Puntaje = int.TryParse(f["PuntajeSatisfacción"], out int puntaje) ? puntaje : 0
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.IdExterno) &&
                        x.IdClienteValido &&
                        x.IdProductoValido &&
                        x.FechaValida &&
                        x.PuntajeValido &&
                        ValidacionHelper.EsPuntajeValido(x.Puntaje) &&
                        !string.IsNullOrWhiteSpace(x.Comentario) &&
                        clientes.Contains(x.IdCliente) &&
                        productos.Contains(x.IdProducto) &&
                        !duplicados.Contains(x.IdExterno))
                    .GroupBy(x => x.IdExterno)
                    .Select(g => g.First())
                    .Select(x => new Opinion
                    {
                        IdExterno = x.IdExterno,
                        TipoOpinion = "ENCUESTA",
                        IdCliente = x.IdCliente,
                        IdProducto = x.IdProducto,
                        IdFuente = fuente.IdFuente,
                        Fecha = x.Fecha,
                        Comentario = x.Comentario,
                        Clasificacion = x.Clasificacion,
                        PuntajeSatisfaccion = x.Puntaje,
                        Rating = null
                    })
                    .ToList();

                _context.Opiniones.AddRange(opiniones);
                _context.SaveChanges();

                resultado.Insertados = opiniones.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("surveys_part1.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }

        public ResultadoCarga CargarWebReviews(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "web_reviews.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var clientes = _context.Clientes.AsNoTracking().Select(c => c.IdCliente).ToHashSet();
                var productos = _context.Productos.AsNoTracking().Select(p => p.IdProducto).ToHashSet();
                var fuente = _fuenteDatoService.ObtenerOCrearFuente("Web");

                var duplicados = _context.Opiniones
                    .AsNoTracking()
                    .Where(o => o.TipoOpinion == "WEB")
                    .Select(o => o.IdExterno)
                    .ToHashSet();

                var opiniones = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdReview", "IdCliente", "IdProducto", "Fecha", "Comentario", "Rating"))
                    .Select(f => new
                    {
                        IdExterno = TransformacionHelper.LimpiarTexto(f["IdReview"]),
                        IdCliente = TransformacionHelper.ConvertirIdConPrefijo(f["IdCliente"], "C"),
                        IdProducto = TransformacionHelper.ConvertirIdConPrefijo(f["IdProducto"], "P"),
                        FechaValida = DateTime.TryParse(f["Fecha"], out DateTime _),
                        Fecha = DateTime.TryParse(f["Fecha"], out DateTime fecha) ? fecha : DateTime.MinValue,
                        Comentario = TransformacionHelper.LimpiarTexto(f["Comentario"]),
                        RatingValido = int.TryParse(f["Rating"], out int _),
                        Rating = int.TryParse(f["Rating"], out int rating) ? rating : 0
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.IdExterno) &&
                        x.IdCliente.HasValue &&
                        x.IdProducto.HasValue &&
                        x.FechaValida &&
                        x.RatingValido &&
                        ValidacionHelper.EsPuntajeValido(x.Rating) &&
                        !string.IsNullOrWhiteSpace(x.Comentario) &&
                        clientes.Contains(x.IdCliente.Value) &&
                        productos.Contains(x.IdProducto.Value) &&
                        !duplicados.Contains(x.IdExterno))
                    .GroupBy(x => x.IdExterno)
                    .Select(g => g.First())
                    .Select(x => new Opinion
                    {
                        IdExterno = x.IdExterno,
                        TipoOpinion = "WEB",
                        IdCliente = x.IdCliente!.Value,
                        IdProducto = x.IdProducto!.Value,
                        IdFuente = fuente.IdFuente,
                        Fecha = x.Fecha,
                        Comentario = x.Comentario,
                        Clasificacion = null,
                        PuntajeSatisfaccion = null,
                        Rating = x.Rating
                    })
                    .ToList();

                _context.Opiniones.AddRange(opiniones);
                _context.SaveChanges();

                resultado.Insertados = opiniones.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("web_reviews.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }

        public ResultadoCarga CargarSocialComments(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "social_comments.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var clientes = _context.Clientes.AsNoTracking().Select(c => c.IdCliente).ToHashSet();
                var productos = _context.Productos.AsNoTracking().Select(p => p.IdProducto).ToHashSet();

                var duplicados = _context.Opiniones
                    .AsNoTracking()
                    .Where(o => o.TipoOpinion == "SOCIAL")
                    .Select(o => o.IdExterno)
                    .ToHashSet();

                var opiniones = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdComment", "IdCliente", "IdProducto", "Fuente", "Fecha", "Comentario"))
                    .Select(f =>
                    {
                        var nombreFuente = string.IsNullOrWhiteSpace(f["Fuente"])
                            ? "Social"
                            : TransformacionHelper.LimpiarTexto(f["Fuente"]);

                        var fuente = _fuenteDatoService.ObtenerOCrearFuente(nombreFuente);

                        return new
                        {
                            IdExterno = TransformacionHelper.LimpiarTexto(f["IdComment"]),
                            IdCliente = TransformacionHelper.ConvertirIdConPrefijo(f["IdCliente"], "C"),
                            IdProducto = TransformacionHelper.ConvertirIdConPrefijo(f["IdProducto"], "P"),
                            IdFuente = fuente.IdFuente,
                            FechaValida = DateTime.TryParse(f["Fecha"], out DateTime _),
                            Fecha = DateTime.TryParse(f["Fecha"], out DateTime fecha) ? fecha : DateTime.MinValue,
                            Comentario = TransformacionHelper.LimpiarTexto(f["Comentario"])
                        };
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.IdExterno) &&
                        x.IdCliente.HasValue &&
                        x.IdProducto.HasValue &&
                        x.FechaValida &&
                        !string.IsNullOrWhiteSpace(x.Comentario) &&
                        clientes.Contains(x.IdCliente.Value) &&
                        productos.Contains(x.IdProducto.Value) &&
                        !duplicados.Contains(x.IdExterno))
                    .GroupBy(x => x.IdExterno)
                    .Select(g => g.First())
                    .Select(x => new Opinion
                    {
                        IdExterno = x.IdExterno,
                        TipoOpinion = "SOCIAL",
                        IdCliente = x.IdCliente!.Value,
                        IdProducto = x.IdProducto!.Value,
                        IdFuente = x.IdFuente,
                        Fecha = x.Fecha,
                        Comentario = x.Comentario,
                        Clasificacion = null,
                        PuntajeSatisfaccion = null,
                        Rating = null
                    })
                    .ToList();

                _context.Opiniones.AddRange(opiniones);
                _context.SaveChanges();

                resultado.Insertados = opiniones.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("social_comments.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }
    }
}