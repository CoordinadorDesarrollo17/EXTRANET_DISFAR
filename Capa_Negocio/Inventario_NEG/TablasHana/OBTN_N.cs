using Capa_Datos.Inventario_DAO.TablasHana;
using Capa_Entidad.Inventario_ENT.TablasHana;
using System.Collections.Generic;
using System.Linq;
namespace Capa_Negocio.Inventario_NEG.TablasHana
{
    public class OBTN_N
    {
        OBTN_D _datos = new OBTN_D();
        public string SelectLotesArticulo(string ItemCode)
        {
            return _datos.SelectLotesArticulo(ItemCode);
        }

        public List<object> ListarLotesJson(string ItemCode)
        { 
            return new OBTN_D().ListarLotesArticulo(ItemCode).Select(x => new
            {
                value = x.DistNumber,
                text = x.DistNumber,
                mnfSerial = x.MnfSerial,
                expDate = x.ExpDate.ToString("yyyy-MM-dd")
            }).ToList<object>();
        }
        public List<OBTN_E> ObtenerRegistrosSanitariosExistentes(OBTN_E filtros = null)
        {
            return _datos.ObtenerRegistrosSanitariosExistentes(filtros);
        }
    }
}
