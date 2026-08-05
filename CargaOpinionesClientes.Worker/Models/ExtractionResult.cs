namespace CargaOpinionesClientes.Worker.Models;

public class ExtractionResult
{
    public string SourceName { get; set; } = string.Empty;

    public bool Success { get; set; }

    public List<ExtractedRecord> Records { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public int TotalRecords => Records.Count;
}