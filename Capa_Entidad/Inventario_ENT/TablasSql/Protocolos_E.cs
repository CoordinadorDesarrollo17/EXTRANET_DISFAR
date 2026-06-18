using System;
using System.Web;
namespace Capa_Entidad.Inventario_ENT.TablasSql
{
    public class Protocolos_E
    {
        public string ItemCode { get; set; }
        public string DistNumber { get; set; }
        public string Ruta { get; set; }
        public string OpCarga { get; set; }
        public DateTime TiempoCarga { get; set; }
        //campos no de la tabla
        public HttpPostedFileBase ArchivoPdf { get; set; }  
        public string ItemName { get; set; }
        public int FirmCode { get; set; }
        public string MnfSerial { get; set; }
        public string ExpDate { get; set; }
        public string RutaArchivo { get; set; }
    }
}
