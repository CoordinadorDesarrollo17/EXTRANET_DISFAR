using Capa_Entidad;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
namespace Capa_Negocio
{
    public class Helpers
    {
        public string SanitizarTexto(string texto, bool eliminarEspacios = false)
        {
            if (string.IsNullOrEmpty(texto))
                return texto;

            // 1. Normalizar para quitar acentos
            string normalized = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            string textoSinAcentos = sb.ToString().Normalize(NormalizationForm.FormC);

            // 2. Opcionalmente eliminar espacios
            if (eliminarEspacios)
            {
                textoSinAcentos = textoSinAcentos.Replace(" ", "");
            }

            // 3. Limpiar texto dejando letras, números, espacios (si no se eliminan),
            // y símbolos comunes como: -, ., ,, +, %
            string pattern = eliminarEspacios
                ? @"[^a-zA-Z0-9\-,\.\+\%]"
                : @"[^a-zA-Z0-9\s\-,\.\+\%]";

            textoSinAcentos = Regex.Replace(textoSinAcentos, pattern, string.Empty);

            return textoSinAcentos;
        }

        // Devolverá true si todos los caracteres en la cadena son números y false en caso contrario.
        public bool EsNumero(string cadena)
        {
            foreach (char caracter in cadena)
            {
                if (!char.IsDigit(caracter))
                {
                    return false;
                }
            }
            return true;
        }
        public bool EsCorreo(string correoElectronico)
        {
            const string expresionRegular = @"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$";
            if (Regex.IsMatch(correoElectronico, expresionRegular))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool ContieneCaracterEspecial(string input)
        {
            // Expresión regular para verificar caracteres especiales
            string pattern = @"[!@#$%^&*?""{}|<>]";
            // Crear la instancia de Regex
            Regex regex = new Regex(pattern);
            // Retorna true si encuentra al menos un carácter especial
            return regex.IsMatch(input);
        }
        public Helper_E CrearRespuestaError(string mensaje)
        {
            return new Helper_E
            {
                Mensaje = mensaje,
                IconoSweetAlert = "error"
            };
        }


        public static string VerificacionAccesos(Controller controller, int ope)
        {
            string nombreOperacion = controller.ControllerContext.RouteData.Values["action"].ToString();
            var user = (Usuario_E)controller.HttpContext.Session["Usuario"];

            if (user == null)
                return "E_Login";

            var oeouN = new Capa_Negocio.Seguridad_NEG.TablasSql.UsuarioOperacion_N();
            if ((oeouN.VerificarAccesoOperacion(user.TipoUsuarioId, ope, nombreOperacion) == 1) || (user.TipoUsuarioId == 1) 
                || (user.TipoUsuarioId == 2))
                return "C_Access";

            return "E_Access";
        }

    }
}
