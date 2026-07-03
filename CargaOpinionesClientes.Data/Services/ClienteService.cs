using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Helpers;
using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Models;
using CargaOpinionesClientes.Data.Result;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Data.Services
{
    public class ClienteService : IClienteService
    {
        private readonly OpinionesDbContext _context;
        private readonly IErrorCargaService _errorService;

        public ClienteService(OpinionesDbContext context, IErrorCargaService errorService)
        {
            _context = context;
            _errorService = errorService;
        }

        public ResultadoCarga CargarClientes(string ruta)
        {
            var resultado = new ResultadoCarga { Archivo = "clients.csv" };

            try
            {
                var filas = CsvHelperService.LeerCsv(ruta);
                resultado.Procesados = filas.Count;

                var idsExistentes = _context.Clientes
                    .AsNoTracking()
                    .Select(c => c.IdCliente)
                    .ToHashSet();

                var emailsExistentes = _context.Clientes
                    .AsNoTracking()
                    .Select(c => c.Email)
                    .ToHashSet();

                var clientes = filas
                    .Where(f => ValidacionHelper.TieneCampos(f, "IdCliente", "Nombre", "Email"))
                    .Select(f => new
                    {
                        Valido = int.TryParse(f["IdCliente"], out int _),
                        IdCliente = int.TryParse(f["IdCliente"], out int id) ? id : 0,
                        Nombre = TransformacionHelper.LimpiarTexto(f["Nombre"]),
                        Email = TransformacionHelper.LimpiarTexto(f["Email"])
                    })
                    .Where(x =>
                        x.Valido &&
                        !string.IsNullOrWhiteSpace(x.Nombre) &&
                        ValidacionHelper.EsEmailValido(x.Email) &&
                        !idsExistentes.Contains(x.IdCliente) &&
                        !emailsExistentes.Contains(x.Email))
                    .GroupBy(x => x.IdCliente)
                    .Select(g => g.First())
                    .Select(x => new Cliente
                    {
                        IdCliente = x.IdCliente,
                        Nombre = x.Nombre,
                        Email = x.Email
                    })
                    .ToList();

                _context.Clientes.AddRange(clientes);
                _context.SaveChanges();

                resultado.Insertados = clientes.Count;
                resultado.Rechazados = resultado.Procesados - resultado.Insertados;
            }
            catch (Exception ex)
            {
                _errorService.GuardarError("clients.csv", null, ex.Message, null);
                resultado.Rechazados = resultado.Procesados;
            }

            return resultado;
        }
    }
}