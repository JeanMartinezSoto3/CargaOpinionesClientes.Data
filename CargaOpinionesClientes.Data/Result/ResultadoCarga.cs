using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargaOpinionesClientes.Data.Result
{
    public class ResultadoCarga
    {
        public string Archivo { get; set; } = string.Empty;

        public int Procesados { get; set; }

        public int Insertados { get; set; }

        public int Rechazados { get; set; }

        public void Mostrar()
        {
            Console.WriteLine("-------------------------------------");
            Console.WriteLine($"Archivo: {Archivo}");
            Console.WriteLine($"Procesados: {Procesados}");
            Console.WriteLine($"Insertados: {Insertados}");
            Console.WriteLine($"Rechazados: {Rechazados}");
            Console.WriteLine("-------------------------------------");
        }
    }
}
