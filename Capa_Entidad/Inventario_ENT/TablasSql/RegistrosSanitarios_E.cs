using System;
using System.Web;
namespace Capa_Entidad.Inventario_ENT.TablasSql
{
    public class RegistrosSanitarios_E
    {
        public string MnfSerial { get; set; }
        public string Ruta { get; set; }
        public string OpCarga { get; set; }
        public DateTime TiempoCarga { get; set; }
        
        // ✅ Campos de otras tablas (requieren JOIN)
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
      
        //campos no de la tabla
        public HttpPostedFileBase ArchivoPdf { get; set; }
        public string RutaArchivo { get; set; }
    }
}
