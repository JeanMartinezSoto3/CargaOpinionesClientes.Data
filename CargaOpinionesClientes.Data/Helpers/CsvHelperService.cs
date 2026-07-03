using System.Text;

namespace CargaOpinionesClientes.Data.Helpers
{
    public static class CsvHelperService
    {
        public static List<Dictionary<string, string>> LeerCsv(string ruta)
        {
            var lineas = File.ReadAllLines(ruta, Encoding.UTF8);

            if (lineas.Length == 0)
                return new List<Dictionary<string, string>>();

            var encabezados = SepararLineaCsv(lineas[0]);

            return lineas
                .Skip(1)
                .Where(linea => !string.IsNullOrWhiteSpace(linea))
                .Select(linea =>
                {
                    var valores = SepararLineaCsv(linea);
                    var diccionario = new Dictionary<string, string>();

                    for (int i = 0; i < encabezados.Count; i++)
                    {
                        string valor = i < valores.Count ? valores[i] : string.Empty;
                        diccionario[encabezados[i].Trim()] = valor.Trim();
                    }

                    return diccionario;
                })
                .ToList();
        }

        private static List<string> SepararLineaCsv(string linea)
        {
            var resultado = new List<string>();
            var valorActual = new StringBuilder();
            bool dentroComillas = false;

            foreach (char caracter in linea)
            {
                if (caracter == '"')
                {
                    dentroComillas = !dentroComillas;
                }
                else if (caracter == ',' && !dentroComillas)
                {
                    resultado.Add(valorActual.ToString());
                    valorActual.Clear();
                }
                else
                {
                    valorActual.Append(caracter);
                }
            }

            resultado.Add(valorActual.ToString());

            return resultado;
        }
    }
}