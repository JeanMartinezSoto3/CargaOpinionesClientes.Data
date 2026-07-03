using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Helpers;
using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Models;
using CargaOpinionesClientes.Data.Result;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Data.Services
{
    public class ProductoService : IProductoService
    {
        private readonly OpinionesDbContext _context;
        private readonly IErrorCargaService _errorService;

        public ProductoService(OpinionesDbContext context, IErrorCargaService errorService)
        {
            _context = context;
            _errorService = errorService;
        }

        public ResultadoCarga CargarProductos(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "products.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var idsExistentes = _context.Productos
                    .AsNoTracking()
                    .Select(p => p.IdProducto)
                    .ToHashSet();

                var productos = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdProducto", "Nombre", "Categoría"))
                    .Select(f => new
                    {
                        Valido = int.TryParse(f["IdProducto"], out int _),
                        IdProducto = int.TryParse(f["IdProducto"], out int id) ? id : 0,
                        Nombre = TransformacionHelper.LimpiarTexto(f["Nombre"]),
                        Categoria = TransformacionHelper.LimpiarTexto(f["Categoría"])
                    })
                    .Where(x =>
                        x.Valido &&
                        !string.IsNullOrWhiteSpace(x.Nombre) &&
                        !string.IsNullOrWhiteSpace(x.Categoria) &&
                        !idsExistentes.Contains(x.IdProducto))
                    .GroupBy(x => x.IdProducto)
                    .Select(g => g.First())
                    .Select(x => new Producto
                    {
                        IdProducto = x.IdProducto,
                        Nombre = x.Nombre,
                        Categoria = x.Categoria
                    })
                    .ToList();

                _context.Productos.AddRange(productos);
                _context.SaveChanges();

                resultado.Insertados = productos.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("products.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }
    }
}