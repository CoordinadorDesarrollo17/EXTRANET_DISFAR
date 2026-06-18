using Capa_Entidad.SociosNegocios_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
namespace Capa_Datos.SociosNegocios_DAO.TablasSql
{
    public class Clientes_D
    {
        DBHelper db = new DBHelper(); 
        public List<Clientes_E> ListarClientes(Clientes_E filtro, bool tienenUsuarios)
        {
            List<Clientes_E> lista = new List<Clientes_E>();
            string query = "SELECT CLI.Id, CLI.CardCode, CLI.CardName, CLI.CardType, CLI.E_Mail, CONVERT(varchar, CLI.CreateDate, 103)  FROM Clientes CLI";
            if (tienenUsuarios == false)
                query += " WHERE NOT EXISTS (SELECT 1 FROM Usuarios USU WHERE USU.Usuario = CLI.CardCode)";
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query);
                while (dr.Read())
                {
                    Clientes_E o = new Clientes_E();
                    if (!dr.IsDBNull(0)) { o.Id = dr.GetInt32(0); }
                    if (!dr.IsDBNull(1)) { o.CardCode = dr.GetString(1); }
                    if (!dr.IsDBNull(2)) { o.CardName = dr.GetString(2); }
                    if (!dr.IsDBNull(3)) { o.CardType = dr.GetString(3); }
                    if (!dr.IsDBNull(4)) { o.E_Mail = dr.GetString(4); }
                    if (!dr.IsDBNull(5)) { o.CreateDate = dr.GetString(5); }
                    lista.Add(o);
                }
                dr.Close();
            }
            catch { }
            return lista;
        }
    }
}
