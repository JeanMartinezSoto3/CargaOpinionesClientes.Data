namespace CargaOpinionesClientes.Data.Interfaces
{
    public interface IErrorCargaService
    {
        void GuardarError(string archivo, int? fila, string motivo, string? datos);
    }
}