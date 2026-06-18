using System;
namespace Capa_Entidad.Ventas_ENT.Formularios
{
    public class MigComprobantes_E
    {
        public TimeSpan Intervalo { get; set; }
        // para rangos between
        public string FecIni { get; set; }
        public string FecFin { get; set; }
        //para >=
        public string FecMin { get; set; }
        //para <=
        public string FecMax { get; set; }
        //para ==
        public DateTime Dia { get; set; }
        // para >
        public int DocEntryMin { get; set; }
        public int DocEntryMax { get; set; }
        public string ObjType { get; set; }
        public int NroForms { get; set; }
    }
}
