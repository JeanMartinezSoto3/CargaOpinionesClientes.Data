namespace CargaOpinionesClientes.Worker.Interfaces;

public interface IFactLoadService
{
    Task LoadFactsAsync(
        CancellationToken cancellationToken);
}