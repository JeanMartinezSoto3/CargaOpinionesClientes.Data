namespace CargaOpinionesClientes.Worker.Models.Extraction;

public class ExtractedRecord
{
    public string Source { get; set; } = string.Empty;

    public string DataType { get; set; } = string.Empty;

    public Dictionary<string, object?> Data { get; set; } = new();
}