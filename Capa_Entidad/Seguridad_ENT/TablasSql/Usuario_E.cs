using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Capa_Entidad.Seguridad_ENT.TablasSql
{
    public class Usuario_E
    {
        public int Id { get; set; }
        public string Usuario { get; set; }
        public string Nombre { get; set; }
        public string Contrasena { get; set; }
        public int TipoUsuarioId { get; set; }
        public string OpCreacion { get; set; }
        public DateTime? TiempoCreacion { get; set; }
        // Campos que no son de la tabla
        public string NombreTipoUsuario { get; set; }
        public string NuevaContrasena { get; set; }
        public string NuevaContrasena2 { get; set; }

    }
}
