using CargaOpinionesClientes.Data.Result;

namespace CargaOpinionesClientes.Data.Interfaces
{
    public interface IProductoService
    {
        ResultadoCarga CargarProductos(string ruta);
    }
}