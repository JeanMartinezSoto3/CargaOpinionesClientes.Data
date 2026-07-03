using CargaOpinionesClientes.Data.Result;

namespace CargaOpinionesClientes.Data.Interfaces
{
    public interface IOpinionService
    {
        ResultadoCarga CargarEncuestas(string ruta);

        ResultadoCarga CargarWebReviews(string ruta);

        ResultadoCarga CargarSocialComments(string ruta);
    }
}