using System.Collections.Generic;
using System.Data.SqlClient;
using Capa_Entidad.Seguridad_ENT.TablasSql;
namespace Capa_Datos.Seguridad_DAO.TablasMysql
{
    public class TipoUsuario_D
    {
        DBHelper db = new DBHelper();
        public string BuscarRutaManual(int IdTipo)
        {
            TipoUsuario_E o = new TipoUsuario_E();
            string query = "select RutaManual from TipoUsuario where Id=" + IdTipo;
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query);
                dr.Read();
                if (!dr.IsDBNull(0)) { o.RutaManual = dr.GetString(0); }
                dr.Close();
            }
            catch
            { }
            return o.RutaManual;
        }
        public List<TipoUsuario_E> ListaTiposUsuario()
        {
            List<TipoUsuario_E> lista = new List<TipoUsuario_E>();
            string query = "select distinct Id, Tipo from TipoUsuario";
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query);
                while (dr.Read())
                {
                    TipoUsuario_E o = new TipoUsuario_E();
                    if (!dr.IsDBNull(0)) { o.Id = dr.GetInt32(0); }
                    if (!dr.IsDBNull(1)) { o.Tipo = dr.GetString(1); }
                    lista.Add(o);
                }
                dr.Close();
            }
            catch
            { }
            return lista;
        }
    }
}
