using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Warehouse;

[Table("DimFuente")]
public class DimFuente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdFuente { get; set; }

    [MaxLength(20)]
    public string? CodigoFuente { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreFuente { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string TipoCanal { get; set; } = string.Empty;
}