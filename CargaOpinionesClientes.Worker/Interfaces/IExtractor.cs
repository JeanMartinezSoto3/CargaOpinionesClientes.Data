using CargaOpinionesClientes.Worker.Models.Extraction;

namespace CargaOpinionesClientes.Worker.Interfaces;

public interface IExtractor
{
    Task<List<ExtractionResult>> ExtractAsync(
        CancellationToken cancellationToken);
}