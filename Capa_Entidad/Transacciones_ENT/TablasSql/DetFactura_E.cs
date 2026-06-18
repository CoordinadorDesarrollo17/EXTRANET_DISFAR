using Capa_Entidad.Inventario_ENT.TablasSql;
namespace Capa_Entidad.Transacciones_ENT.TablasSql
{
    public class DetFactura_E
    {
        public string ItemCode { get; set; }
        public string BatchNum { get; set; }
        public int LineNum { get; set; }
        public string WhsCode { get; set; }
        public string ItemName { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public RegistrosSanitarios_E RegistroSanitario { get; set; }
        public EspecificacionesTecnicas_E ET { get; set; }
        public Protocolos_E Protocolo { get; set; }
        public bool ExisteRs(string ruta)
        {
            if (RegistroSanitario == null || string.IsNullOrEmpty(ruta))
                return false;
            if (System.IO.File.Exists(ruta)) { return true; } else { return false; }
        }
        public bool ExistePt(string ruta)
        {
            if (Protocolo == null || string.IsNullOrEmpty(ruta))
                return false;
            if (System.IO.File.Exists(ruta)) { return true; } else { return false; }
        }
        public bool ExisteEt(string ruta)
        {
            if (ET == null || string.IsNullOrEmpty(ruta))
                return false;
            if (System.IO.File.Exists(ruta)) { return true; } else { return false; }
        }
    }
}
