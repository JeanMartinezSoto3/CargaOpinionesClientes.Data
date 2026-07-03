using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaOpinionesClientes.Data.Models
{
    public class FuenteDato
    {
        public int IdFuente { get; set; }

        public string? CodigoFuente { get; set; }

        public string NombreFuente { get; set; } = string.Empty;

        public DateTime? FechaCarga { get; set; }

        public ICollection<Opinion> Opiniones { get; set; } = new List<Opinion>();
    }
}