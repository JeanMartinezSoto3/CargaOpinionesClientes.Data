using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Warehouse;

[Table("DimFecha")]
public class DimFecha
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdFecha { get; set; }

    [Column(TypeName = "date")]
    public DateTime Fecha { get; set; }

    public int Dia { get; set; }

    public int Mes { get; set; }

    [Required]
    [MaxLength(20)]
    public string NombreMes { get; set; } = string.Empty;

    public int Trimestre { get; set; }

    public int Anio { get; set; }
}