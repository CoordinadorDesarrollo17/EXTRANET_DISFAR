using System;
using System.Web;
namespace Capa_Entidad.Inventario_ENT.TablasSql
{
    public class EspecificacionesTecnicas_E
    {
        public string ItemCode { get; set; }
        public string Ruta { get; set; }
        public string Nombre { get; set; }
        public string OpCarga { get; set; }
        public DateTime TiempoCarga { get; set; }
        //campos no de la tabla
        public HttpPostedFileBase ArchivoPdf { get; set; }
        public string ItemName { get; set; }
        public int FirmCode { get; set; }
        public string RutaArchivo { get; set; }
    }
}
