using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Helpers;
using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Models;
using CargaOpinionesClientes.Data.Result;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Data.Services
{
    public class FuenteDatoService : IFuenteDatoService
    {
        private readonly OpinionesDbContext _context;
        private readonly IErrorCargaService _errorService;

        public FuenteDatoService(OpinionesDbContext context, IErrorCargaService errorService)
        {
            _context = context;
            _errorService = errorService;
        }

        public ResultadoCarga CargarFuentes(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "fuente_datos.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var fuentesExistentes = _context.FuentesDatos
                    .AsNoTracking()
                    .Select(f => f.NombreFuente)
                    .ToHashSet();

                var fuentes = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdFuente", "TipoFuente", "FechaCarga"))
                    .Select(f => new
                    {
                        CodigoFuente = TransformacionHelper.LimpiarTexto(f["IdFuente"]),
                        NombreFuente = TransformacionHelper.LimpiarTexto(f["TipoFuente"]),
                        FechaCarga = DateTime.TryParse(f["FechaCarga"], out DateTime fecha)
                            ? fecha
                            : (DateTime?)null
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.NombreFuente) &&
                        !fuentesExistentes.Contains(x.NombreFuente))
                    .GroupBy(x => x.NombreFuente)
                    .Select(g => g.First())
                    .Select(x => new FuenteDato
                    {
                        CodigoFuente = x.CodigoFuente,
                        NombreFuente = x.NombreFuente,
                        FechaCarga = x.FechaCarga
                    })
                    .ToList();

                _context.FuentesDatos.AddRange(fuentes);
                _context.SaveChanges();

                resultado.Insertados = fuentes.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("fuente_datos.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }

        public FuenteDato ObtenerOCrearFuente(string nombreFuente)
        {
            var fuente = _context.FuentesDatos
                .FirstOrDefault(f => f.NombreFuente == nombreFuente);

            if (fuente != null)
                return fuente;

            fuente = new FuenteDato
            {
                NombreFuente = nombreFuente,
                FechaCarga = DateTime.Now
            };

            _context.FuentesDatos.Add(fuente);
            _context.SaveChanges();

            return fuente;
        }
    }
}