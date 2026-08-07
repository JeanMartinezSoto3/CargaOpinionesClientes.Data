using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Transactional;

[Table("FuentesDatos")]
public class FuenteDato
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdFuente { get; set; }

    [MaxLength(20)]
    public string? CodigoFuente { get; set; }

    [Required]
    [MaxLength(100)]
    public string NombreFuente { get; set; } = string.Empty;

    [Column(TypeName = "date")]
    public DateTime? FechaCarga { get; set; }
}