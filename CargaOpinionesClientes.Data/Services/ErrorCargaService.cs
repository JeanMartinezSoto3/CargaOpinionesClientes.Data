using CargaOpinionesClientes.Data.Context;
using CargaOpinionesClientes.Data.Interfaces;
using CargaOpinionesClientes.Data.Models;

namespace CargaOpinionesClientes.Data.Services
{
    public class ErrorCargaService : IErrorCargaService
    {
        private readonly OpinionesDbContext _context;

        public ErrorCargaService(OpinionesDbContext context)
        {
            _context = context;
        }

        public void GuardarError(string archivo, int? fila, string motivo, string? datos)
        {
            try
            {
                _context.ErroresCarga.Add(new ErrorCarga
                {
                    Archivo = archivo,
                    Fila = fila,
                    Motivo = motivo,
                    DatosRegistro = datos
                });

                _context.SaveChanges();
            }
            catch
            {
                Console.WriteLine($"No se pudo guardar el error del archivo {archivo}");
            }
        }
    }
}