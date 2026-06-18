using System;
using System.Collections.Generic;
namespace Capa_Entidad.Transacciones_ENT.TablasHana
{
    public class OINV_E
    {
        public int DocEntry { get; set; }
        public string CardCode { get; set; }
        public string Correlativo { get; set; }
        public DateTime DocDate { get; set; }
        public decimal DocTotal { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public List<INV1_E> Detalle { get; set; }
    }
}
