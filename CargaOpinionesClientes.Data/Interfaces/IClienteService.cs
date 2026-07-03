using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargaOpinionesClientes.Data.Result;
using CargaOpinionesClientes.Data.Result;

namespace CargaOpinionesClientes.Data.Interfaces
{
    public interface IClienteService
    {
        ResultadoCarga CargarClientes(string ruta);
    }
}