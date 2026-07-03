using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaOpinionesClientes.Data.Models
{
    public class ErrorCarga
    {
        public int IdError { get; set; }

        public string Archivo { get; set; } = string.Empty;

        public int? Fila { get; set; }

        public string Motivo { get; set; } = string.Empty;

        public string? DatosRegistro { get; set; }

        public DateTime FechaError { get; set; }
    }
}