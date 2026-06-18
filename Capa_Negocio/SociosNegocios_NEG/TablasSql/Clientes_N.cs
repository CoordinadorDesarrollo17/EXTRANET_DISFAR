using Capa_Datos.SociosNegocios_DAO.TablasSql;
using Capa_Entidad.SociosNegocios_ENT.TablasSql;
using System.Collections.Generic;
namespace Capa_Negocio.SociosNegocios_NEG.TablasSql
{
    public class Clientes_N
    {
        Clientes_D _datos = new Clientes_D();
        public List<Clientes_E> ListarClientes(Clientes_E filtro, bool tienenUsuarios = true)
        {
            return _datos.ListarClientes(filtro, tienenUsuarios);
        }
    }
}
