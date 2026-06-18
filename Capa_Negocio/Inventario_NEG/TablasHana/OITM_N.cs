using Capa_Datos.Inventario_DAO.TablasHana;
using Capa_Entidad.Inventario_ENT.TablasHana;
using System.Collections.Generic;
using System.Linq;
namespace Capa_Negocio.Inventario_NEG.TablasHana
{
    public class OITM_N
    {
        OITM_D _datos = new OITM_D();
        public List<object> ListarArticulosJson(OITM_E fil)
        {
            return new OITM_D().ListarArticulos(fil).Select(x => new
            {
                value = x.ItemCode,
                text = x.ItemName,
                itemName = x.ItemName,
                firmCode = x.FirmCode
            }).ToList<object>();
        }

    }
}
