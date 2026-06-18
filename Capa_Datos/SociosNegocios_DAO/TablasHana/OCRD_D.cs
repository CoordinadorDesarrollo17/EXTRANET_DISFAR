using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using Capa_Entidad.SociosNegocios_ENT.TablasHana;
using Sap.Data.Hana;
namespace Capa_Datos.SociosNegocios_DAO.TablasHana
{
    public class OCRD_D
    {
        DBHelper db = new DBHelper();
        Utilitarios uti = new Utilitarios();
        public List<OCRD_E> ListarClientes(OCRD_E filtro)
        {
            List<OCRD_E> lista = new List<OCRD_E>();
            string fil = "";
            if(filtro!=null)
            {
                if (filtro.CreateDate != null) { fil += " and \"CreateDate\">='" + filtro.CreateDate.ToString("yyyy-MM-dd") + "'"; }
            }
            string query = "select \"CardCode\",\"CardName\",\"CardType\",\"E_Mail\",\"CreateDate\" " +
                " from "+uti.schemaHana+ "ocrd where \"CardType\"='C' "+fil+" order by \"CreateDate\"";
            try
            {
                HanaDataReader hdr = db.HanaExecuteReaderNoSp(query);
                while(hdr.Read())
                {
                    OCRD_E o = new OCRD_E();
                    if (!hdr.IsDBNull(0)) { o.CardCode = hdr.GetString(0); }
                    if (!hdr.IsDBNull(1)) { o.CardName = hdr.GetString(1); }
                    if (!hdr.IsDBNull(2)) { o.CardType = hdr.GetString(2); }
                    if (!hdr.IsDBNull(3)) { o.E_Mail = hdr.GetString(3); }
                    if (!hdr.IsDBNull(4)) { o.CreateDate = hdr.GetDateTime(4); }
                    lista.Add(o);
                }
                hdr.Close();
            }
            catch { }
            return lista;
        }
        public int MigrarClientes(OCRD_E filtro)
        {
            int status = -1;
            string query = "insert into Clientes values(@CardCode,@CardName,@CardType,@E_Mail,@CreateDate);";
            OCRD_D orcdDHana = new OCRD_D();
            SqlConnection cn = new SqlConnection(uti.cadSql);
            try
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();
                try
                {
                    foreach (OCRD_E o in orcdDHana.ListarClientes(filtro))
                    {
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cmd.Transaction = tran;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@CardCode", o.CardCode);
                        cmd.Parameters.AddWithValue("@CardName", o.CardName);
                        cmd.Parameters.AddWithValue("@CardType", o.CardType);
                        cmd.Parameters.AddWithValue("@E_Mail", o.E_Mail);
                        cmd.Parameters.AddWithValue("@CreateDate", o.CreateDate);
                        cmd.ExecuteNonQuery();
                    }
                    tran.Commit();
                    status = 1;
                    cn.Close();
                }
                catch { tran.Rollback(); cn.Close(); throw new Exception("Error en migracion: "); }
            }
            catch (Exception e2) { cn.Close(); status = 0; throw new Exception("Error en conexion: " + e2.Message); }
            return status;
        }
    }
}
