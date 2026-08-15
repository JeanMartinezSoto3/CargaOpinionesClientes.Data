using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Warehouse;

[Table("FactOpiniones")]
public class FactOpinion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdOpinion { get; set; }

    [Required]
    [MaxLength(50)]
    public string IdExterno { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string TipoOpinion { get; set; } = string.Empty;

    public int IdCliente { get; set; }

    public int IdProducto { get; set; }

    public int IdFuente { get; set; }

    public int IdFecha { get; set; }

    [Required]
    [MaxLength(1000)]
    public string Comentario { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Clasificacion { get; set; }

    public int? PuntajeSatisfaccion { get; set; }

    public int? Rating { get; set; }

    public DateTime FechaRegistro { get; set; }
}