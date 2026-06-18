using System.IO;
namespace Capa_Entidad.Inventario_ENT.TablasSql
{
    public class LotesRegistroSanitario_E
    {
        public string ItemCode { get; set; }
        public string DistNumber { get; set; }
        public string MnfSerial { get; set; }
        public string ExpDate { get; set; }
        // Campos que no son de la tabla
        public string ItemName { get; set; }
        public bool ExistRutaRs { get; set; }
        public bool ExistRutaEt { get; set; }
        public bool ExistRutaPt { get; set; }
        public void existeArchivoRs(RegistrosSanitarios_E o, string directorio)
        {
            if (o == null)
            {
                ExistRutaRs = false;
                return;
            }
            string rutaCompleta = Path.Combine(directorio, o.Ruta);
            ExistRutaRs = File.Exists(rutaCompleta);
        }
        public void existeArchivoEt(EspecificacionesTecnicas_E o, string directorio)
        {
            if (o == null)
            {
                ExistRutaEt = false;
                return;
            }
            string rutaCompleta = Path.Combine(directorio, o.Ruta);
            ExistRutaEt = File.Exists(rutaCompleta);
        }
        public void existeArchivoPt(Protocolos_E o, string directorio)
        {
            if (o == null)
            {
                ExistRutaPt = false;
                return;
            }
            string rutaCompleta = Path.Combine(directorio, o.Ruta);
            ExistRutaPt = File.Exists(rutaCompleta);
        }
    }
}