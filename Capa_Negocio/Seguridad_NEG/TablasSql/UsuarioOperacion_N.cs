using Capa_Datos.Seguridad_DAO.TablasMysql;
namespace Capa_Negocio.Seguridad_NEG.TablasSql
{
    public class UsuarioOperacion_N
    {
        UsuarioOperacion_D _datos = new UsuarioOperacion_D();
        public int VerificarAccesoOperacion(int IdTipo, int idOperacion, string nombreOperacion)
        {
            return _datos.VerificarAccesoOperacion(IdTipo, idOperacion, nombreOperacion);
        }
    }
}
