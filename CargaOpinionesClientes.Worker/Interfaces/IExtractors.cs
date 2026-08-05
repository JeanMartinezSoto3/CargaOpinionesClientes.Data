using CargaOpinionesClientes.Worker.Models;

namespace CargaOpinionesClientes.Worker.Interfaces;

public interface IExtractor
{
    Task<List<ExtractionResult>> ExtractAsync(
        CancellationToken cancellationToken);
}