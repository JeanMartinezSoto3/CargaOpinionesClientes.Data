using System.Text.RegularExpressions;

namespace CargaOpinionesClientes.Data.Helpers
{
    public static class ValidacionHelper
    {
        public static bool TieneCampos(Dictionary<string, string> fila, params string[] campos)
        {
            return campos.All(campo => fila.ContainsKey(campo));
        }

        public static bool EsEmailValido(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        public static bool EsPuntajeValido(int puntaje)
        {
            return puntaje >= 1 && puntaje <= 5;
        }
    }
}