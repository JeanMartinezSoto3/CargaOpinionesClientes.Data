using CargaOpinionesClientes.Data.Models;
using CargaOpinionesClientes.Data.Result;

namespace CargaOpinionesClientes.Data.Interfaces
{
    public interface IFuenteDatoService
    {
        ResultadoCarga CargarFuentes(string ruta);

        FuenteDato ObtenerOCrearFuente(string nombreFuente);
    }
}