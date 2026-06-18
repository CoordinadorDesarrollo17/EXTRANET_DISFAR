using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasHana;
using System.IO;
using Capa_Entidad;
namespace Capa_Datos.Transacciones_DAO.TablasSql
{
    public class Factura_D
    {
        DBHelper db = new DBHelper();
        Utilitarios uti = new Utilitarios();
        public (Helper_E, List<Factura_E>) ListarComprobantes(string condicion = "", Dictionary<string, object> parametros = null)
        {
            var lista = new List<Factura_E>();
            Helper_E _helper = new Helper_E();

            var top = string.IsNullOrEmpty(condicion) ? "TOP 100" : string.Empty;

            try
            {
                using (var cn = new SqlConnection(uti.cadSql))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;

                        var sb = new StringBuilder();
                        sb.AppendLine($"SELECT {top} FT.DocEntry, FT.CardCode, Cl.CardName, FT.Correlativo, CONVERT(varchar, FT.DocDate, 103),");
                        sb.AppendLine("FT.DocTotal, FT.BaseType, FT.BaseEntry");
                        sb.AppendLine("FROM Factura FT");
                        sb.AppendLine("INNER JOIN Clientes Cl ON Cl.CardCode = FT.CardCode");
                        sb.AppendLine("WHERE 1 = 1");

                        if (!string.IsNullOrWhiteSpace(condicion))
                        {
                            sb.AppendLine(condicion.Trim());
                        }

                        sb.AppendLine("AND EXISTS (SELECT 1 FROM detfactura DET WHERE DET.BaseEntry = FT.BaseEntry)");
                        sb.AppendLine("ORDER BY FT.DocDate DESC");

                        cmd.CommandText = sb.ToString();

                        // Parámetros dinámicos
                        if (parametros != null)
                        {
                            foreach (var prm in parametros)
                            {
                                cmd.Parameters.AddWithValue(prm.Key, prm.Value ?? DBNull.Value);
                            }
                        }

                        cn.Open();
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var obj = new Factura_E
                                {
                                    DocEntry = !dr.IsDBNull(0) ? dr.GetInt32(0) : 0,
                                    CardCode = !dr.IsDBNull(1) ? dr.GetString(1) : "",
                                    CardName = !dr.IsDBNull(2) ? dr.GetString(2) : "",
                                    Correlativo = !dr.IsDBNull(3) ? dr.GetString(3) : "",
                                    DocDate = !dr.IsDBNull(4) ? dr.GetString(4) : "",
                                    DocTotal = !dr.IsDBNull(5) ? dr.GetDecimal(5) : 0,
                                    BaseType = !dr.IsDBNull(6) ? dr.GetInt32(6) : 0,
                                    BaseEntry = !dr.IsDBNull(7) ? dr.GetInt32(7) : 0
                                };

                                lista.Add(obj);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Error inesperado en Factura_D - ListarComprobantes()");

                _helper.Titulo = "Error";
                _helper.Mensajes.Add("Ocurrió un error al listar comprobantes.");
                _helper.Mensajes.Add("Por favor, comuníquese con el área de Sistemas para más información.");
                _helper.Icono = "error";
            }

            return (_helper, lista);
        }

        public Factura_E BuscarComprobante(int DocEntry, Usuario_E user)
        {
            var o = new Factura_E();
            var query = new StringBuilder();
            var parametros = new List<string> { "@DocEntry" };
            var valores = new List<object> { DocEntry };

            query.AppendLine("SELECT FT.DocEntry, FT.CardCode, FT.Correlativo, CONVERT(varchar, FT.DocDate, 103) AS DocDate, FT.DocTotal, FT.BaseType, FT.BaseEntry");
            query.AppendLine("FROM Factura FT");
            query.AppendLine("WHERE FT.DocEntry = @DocEntry");

            if (user.TipoUsuarioId == 3)
            {
                query.AppendLine("AND FT.CardCode = @CardCode");
                parametros.Add("@CardCode");
            }

            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query.ToString(), parametros, DocEntry, user.Usuario);

                if (dr.Read()) // ✔️ Verifica que haya datos antes de acceder a columnas
                {
                    if (!dr.IsDBNull(0)) o.DocEntry = dr.GetInt32(0);
                    if (!dr.IsDBNull(1)) o.CardCode = dr.GetString(1);
                    if (!dr.IsDBNull(2)) o.Correlativo = dr.GetString(2);
                    if (!dr.IsDBNull(3)) o.DocDate = dr.GetString(3);
                    if (!dr.IsDBNull(4)) o.DocTotal = dr.GetDecimal(4);
                    if (!dr.IsDBNull(5)) o.BaseType = dr.GetInt32(5);
                    if (!dr.IsDBNull(6)) o.BaseEntry = dr.GetInt32(6);

                    o.Detalle = new DetFactura_D().ListaDetalleFactura(o.BaseEntry);
                }

                dr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en BuscarComprobante: " + ex.Message); // solo temporal
            }

            return o;
        }

        public List<int> ComprobantesAnulados()
        {
            List<int> lista = new List<int>();
            TablasHana.OINV_D oinvDHana = new TablasHana.OINV_D();
            string query = "SELECT DocEntry FROM oinv WHERE DocDate BETWEEN @FechaInicio AND @FechaFin";
            SqlConnection cn = new SqlConnection(uti.cadSql);
            try
            {
                cn.Open();
                using (SqlTransaction tran = cn.BeginTransaction())
                {
                    try
                    {
                        // Definir fechas para el rango de búsqueda (dos días hacia atrás y dos días hacia adelante)
                        DateTime fechaInicio = DateTime.Now.Date.AddDays(-2);
                        DateTime fechaFin = DateTime.Now.Date.AddDays(2);
                        // Crear comando MySQL con la consulta y parámetros
                        SqlCommand cmd = new SqlCommand(query, cn);
                        cmd.Parameters.AddWithValue("@FechaInicio", fechaInicio.ToString("yyyy-MM-dd"));
                        cmd.Parameters.AddWithValue("@FechaFin", fechaFin.ToString("yyyy-MM-dd"));
                        // Ejecutar consulta y obtener resultados
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                int docEntry = dr.GetInt32(0);
                                lista.Add(docEntry);
                            }
                        }
                        if (lista.Count > 0)
                        {
                            List<int> listAnulados = oinvDHana.ListarComprobantesAnulados(lista.FirstOrDefault(), lista.LastOrDefault());
                            if (listAnulados.Count > 0)
                            {
                                BorrarComprobantesAnulados(listAnulados);
                            }
                        }
                        tran.Commit();
                    }
                    catch (Exception e)
                    {
                        tran.Rollback();
                        throw new Exception("Error en ComprobantesAnulados: " + e.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en conexión a la base de datos: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
            return lista;
        }
        public int BorrarComprobantesAnulados(List<int> lista)
        {
            int status = -1;
            SqlConnection cn = new SqlConnection(uti.cadSql);
            try
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();
                try
                {
                    string query = @"DELETE t0, t1 
                             FROM oinv t0 
                             INNER JOIN ibt1 t1 ON (t1.BaseEntry = t0.DocEntry AND t1.BaseType = 13) OR (t1.BaseEntry = t0.BaseEntry AND t1.BaseType = 15)  
                             WHERE t0.DocEntry IN (" + string.Join(",", lista) + ")";
                    SqlCommand cmd = new SqlCommand(query, cn);
                    cmd.Transaction = tran;
                    int rowsAffected = cmd.ExecuteNonQuery();
                    tran.Commit();
                    status = 1;
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    throw new Exception("Error en borrarComprobantesAnulados: " + ex.Message);
                }
                finally
                {
                    cn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error en conexión a la base de datos: " + ex.Message);
            }
            finally
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();
            }
            return status;
        }
    }
}
