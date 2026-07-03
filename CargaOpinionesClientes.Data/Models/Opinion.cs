using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaOpinionesClientes.Data.Models
{
    public class Opinion
    {
        public int IdOpinion { get; set; }

        public string IdExterno { get; set; } = string.Empty;

        public string TipoOpinion { get; set; } = string.Empty;

        public int IdCliente { get; set; }

        public int IdProducto { get; set; }

        public int IdFuente { get; set; }

        public DateTime Fecha { get; set; }

        public string Comentario { get; set; } = string.Empty;

        public string? Clasificacion { get; set; }

        public int? PuntajeSatisfaccion { get; set; }

        public int? Rating { get; set; }

        public DateTime FechaRegistro { get; set; }

        public Cliente? Cliente { get; set; }

        public Producto? Producto { get; set; }

        public FuenteDato? FuenteDato { get; set; }
    }
}