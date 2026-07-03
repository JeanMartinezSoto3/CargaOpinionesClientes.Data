using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaOpinionesClientes.Data.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public ICollection<Opinion> Opiniones { get; set; } = new List<Opinion>();
    }
}