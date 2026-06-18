using System.Collections.Generic;
namespace Capa_Entidad
{
    public class Helper_E
    {
        public int DocEntry { get; set; }
        public int Id { get; set; }
        public string Mensaje { get; set; }
        public List<string> Mensajes { get; set; } = new List<string>();
        public string IconoSweetAlert { get; set; }
        public string Titulo { get; set; }
        public string Icono { get; set; }
        public bool Estado { get; set; }
    }
}
