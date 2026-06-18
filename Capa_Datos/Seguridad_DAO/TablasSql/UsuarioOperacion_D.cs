using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Capa_Datos.Seguridad_DAO.TablasMysql
{
    public class UsuarioOperacion_D
    {
        DBHelper db = new DBHelper();
        public int VerificarAccesoOperacion(int IdTipo, int idOperacion, string nombreOperacion)
        {
            int result = -1;
            string query = "SELECT COUNT(*) FROM UsuarioOperacion WHERE TipoUsuarioId=" + IdTipo + " AND OperacionId=" + idOperacion;
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query);
                dr.Read();
                result = dr.GetInt32(0);
                dr.Close();               
            }
            catch{}
            return result;
        }
    }
}
