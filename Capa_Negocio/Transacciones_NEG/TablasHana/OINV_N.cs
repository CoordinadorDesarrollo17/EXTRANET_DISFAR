using Capa_Datos.Transacciones_DAO.TablasHana;
using Capa_Entidad.Transacciones_ENT.TablasHana;
using System.Collections.Generic;
namespace Capa_Negocio.Transacciones_NEG.TablasHana
{
    public class OINV_N
    {
        OINV_D _datos = new OINV_D();
        public List<int> ListarDocEntryComprobantes(string cardCode)
        {
            return _datos.ListarDocEntryComprobantes(cardCode);
        }
        public int ImportarComprobante(OINV_E obj)
        {
            return _datos.ImportarComprobante(obj);
        }
        public OINV_E BuscarComprobante(int docEntry)
        {
            return _datos.BuscarComprobante(docEntry);
        }
    }
}
