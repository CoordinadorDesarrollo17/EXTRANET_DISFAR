using Capa_Datos.Seguridad_DAO.TablasSql;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using System.Collections.Generic;
namespace Capa_Negocio.Seguridad_NEG.TablasSql
{
    public class MenuOpciones_N
    {
        MenuOpciones_D _datos = new MenuOpciones_D();
        public List<MenuOpciones_E> ListarOpcionesMenu(int IdTipo)
        {
            return _datos.ListarOpcionesMenu(IdTipo);
        }
    }
}
