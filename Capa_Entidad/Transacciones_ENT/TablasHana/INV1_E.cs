using System;
namespace Capa_Entidad.Transacciones_ENT.TablasHana
{
    public class INV1_E
    {
        public string ItemCode { get; set; }
        public string BatchNum { get; set; }
        public int LineNum { get; set; }
        public string WhsCode { get; set; }
        public string ItemName { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
