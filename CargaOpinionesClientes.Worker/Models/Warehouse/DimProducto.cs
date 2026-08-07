using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CargaOpinionesClientes.Worker.Models.Warehouse;

[Table("DimProducto")]
public class DimProducto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int IdProducto { get; set; }

    [Required]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Categoria { get; set; } = string.Empty;
}