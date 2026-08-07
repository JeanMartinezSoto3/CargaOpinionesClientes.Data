namespace CargaOpinionesClientes.Worker.Interfaces;

public interface IDimensionLoadService
{
    Task LoadDimensionsAsync(
        CancellationToken cancellationToken);
}