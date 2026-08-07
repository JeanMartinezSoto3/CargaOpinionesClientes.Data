using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Warehouse;

[Table("DimCliente")]
public class DimCliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdCliente { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Pais { get; set; }

    [MaxLength(100)]
    public string? TipoCliente { get; set; }
}