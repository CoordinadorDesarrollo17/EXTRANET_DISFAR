using Capa_Datos.Seguridad_DAO.TablasMysql;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using System.Collections.Generic;
namespace Capa_Negocio.Seguridad_NEG.TablasSql
{
    public class TipoUsuario_N
    {
        TipoUsuario_D _datos = new TipoUsuario_D();
        public string BuscarRutaManual(int IdTipo)
        {
            return _datos.BuscarRutaManual(IdTipo);
        }
        public List<TipoUsuario_E> ListaTiposUsuario()
        {   
            return _datos.ListaTiposUsuario();
        }
    }
}
