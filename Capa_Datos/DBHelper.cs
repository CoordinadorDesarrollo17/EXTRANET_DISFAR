using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Sap.Data.Hana;
namespace Capa_Datos
{
    public class DBHelper
    {
        Utilitarios uti = new Utilitarios();
        public HanaDataReader HanaExecuteReaderNoSp(string query, List<string> npara = null, params object[] Parametros)
        {
            HanaConnection cnx = new HanaConnection(uti.cadHana);
            cnx.Open();
            HanaCommand hcmd = new HanaCommand(query, cnx);
            hcmd.CommandType = CommandType.Text;
            if (npara != null)
            {
                foreach (string p in npara)
                {
                    HanaParameter par = new HanaParameter(p, null);
                    hcmd.Parameters.Add(par);
                }
                if (Parametros.Length > 0)
                    HanaLlenarParametros(hcmd, Parametros);
            }
            HanaDataReader lector = hcmd.ExecuteReader(CommandBehavior.CloseConnection);
            return lector;
        }
        private void HanaLlenarParametros(HanaCommand comando, params object[] parametros)
        {
            int indice = 0;
            int totalParam = parametros.Length;
            HanaCommandBuilder.DeriveParameters(comando);
            //
            foreach (HanaParameter item in comando.Parameters)
            {
                if (item.ParameterName != "@RETURN_VALUE")
                {
                    item.Value = parametros[indice];
                    indice++;
                }
                if (totalParam == (indice)) { return; }
            }
        }
        public SqlConnection SqlConexion ()
        {
            return new SqlConnection(uti.cadSql);
        }
        public SqlDataReader SqlExecuteReaderNoSp(string query, List<string> npara = null, params object[] Parametros)
        {
            SqlConnection cnx = new SqlConnection(uti.cadSql);
            try
            {
                cnx.Open();
                SqlCommand cmd = new SqlCommand(query, cnx);
                cmd.CommandType = CommandType.Text;
                if (npara != null)
                {
                    foreach (string p in npara)
                    {
                        SqlParameter par = new SqlParameter(p, null);
                        cmd.Parameters.Add(par);
                    }
                    if (Parametros.Length > 0)
                        MysqlLlenarParametrosNoSp(cmd, Parametros);
                }
                SqlDataReader lector = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return lector;
            }
            catch(Exception e) { cnx.Close(); throw new Exception(e.Message); }
        }
        public object MysqlExecuteScalarNoSp(string NombreSP)
        {
            SqlConnection cnx = new SqlConnection(uti.cadSql);
            object rpta;
            try
            {
                cnx.Open();
                SqlCommand cmd = new SqlCommand(NombreSP, cnx);
                cmd.CommandType = CommandType.Text;
                rpta = cmd.ExecuteScalar();
                cnx.Close();
            }
            catch (Exception e) { cnx.Close(); throw new Exception(e.Message); }
            return rpta;
        }
        private void MysqlLlenarParametrosNoSp(SqlCommand comando, params object[] parametros)
        {
            int indice = 0;
            foreach (SqlParameter item in comando.Parameters)
            {
                if (item.ParameterName != "@RETURN_VALUE")
                {
                    item.Value = parametros[indice];
                    indice++;
                }
            }
        }
        public void MysqlExecuteNonQueryTrxNoSp(string query, List<string> npara = null, params object[] Parametros)
        {
            SqlConnection cnx = new SqlConnection(uti.cadSql);
            cnx.Open();
            SqlTransaction trx = cnx.BeginTransaction();
            try
            {
                SqlCommand cmd = new SqlCommand(query, cnx, trx);
                cmd.CommandType = CommandType.Text;
                if (npara != null)
                {
                    foreach (string p in npara)
                    {
                        SqlParameter par = new SqlParameter(p, null);
                        cmd.Parameters.Add(par);
                    }
                    if (Parametros.Length > 0)
                        MysqlLlenarParametrosNoSp(cmd, Parametros);
                }
                cmd.ExecuteNonQuery();
                trx.Commit();
                //
                cnx.Close();
            }
            catch (Exception ex)
            {
                trx.Rollback(); // cancela las operaciones
                throw new Exception(ex.Message);
            }
        }
    }
}
