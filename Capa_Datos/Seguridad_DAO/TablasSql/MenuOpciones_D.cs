using Capa_Entidad.Seguridad_ENT.TablasSql;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
namespace Capa_Datos.Seguridad_DAO.TablasSql
{
    public class MenuOpciones_D
    {
        DBHelper db = new DBHelper();
        public List<MenuOpciones_E> ListarOpcionesMenu(int IdTipo)
        {
            List<MenuOpciones_E> lista = new List<MenuOpciones_E>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("SELECT t0.Id, t0.Descripcion, t0.NombreOperacion");
            sb.AppendLine("FROM MenuOpciones t0");
            sb.AppendLine("INNER JOIN OrdenMenuOpciones t1 ON t1.MenuId = t0.Id");
            sb.AppendLine("WHERE t1.TipoUsuarioId = @IdTipo;");

            string query = sb.ToString();
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query,new List<string>() { "@IdTipo" },IdTipo);
                while (dr.Read())
                {
                    MenuOpciones_E o = new MenuOpciones_E();
                    if (!dr.IsDBNull(0)) { o.Id = dr.GetInt32(0); }
                    if (!dr.IsDBNull(1)) { o.Descripcion = dr.GetString(1); }
                    if (!dr.IsDBNull(2)) { o.NombreOperacion = dr.GetString(2); }
                    lista.Add(o);
                }
                dr.Close();
            }
            catch { }
            return lista;
        }
    }
}
