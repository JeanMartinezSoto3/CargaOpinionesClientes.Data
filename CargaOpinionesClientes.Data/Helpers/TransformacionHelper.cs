namespace CargaOpinionesClientes.Data.Helpers
{
    public static class TransformacionHelper
    {
        public static int? ConvertirIdConPrefijo(string valor, string prefijo)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return null;

            valor = valor.Trim().Replace(prefijo, "");

            return int.TryParse(valor, out int id) ? id : null;
        }

        public static string LimpiarTexto(string? texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? string.Empty
                : texto.Trim();
        }
    }
}