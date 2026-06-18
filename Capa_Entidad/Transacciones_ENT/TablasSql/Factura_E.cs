using System.Collections.Generic;
namespace Capa_Entidad.Transacciones_ENT.TablasSql
{
    public class Factura_E
    {
        public int DocEntry { get; set; }
        public string CardCode { get; set; }
        public string Correlativo { get; set; }
        public string DocDate { get; set; }
        public decimal DocTotal { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        // Campos que no son de la tabla
        public string FechaInicio { get; set; }
        public string FechaFin { get; set; }
        public string Correlativo2 { get; set; }
        public List<DetFactura_E> Detalle { get; set; }
        public string CardName { get; set; }
    }
}
