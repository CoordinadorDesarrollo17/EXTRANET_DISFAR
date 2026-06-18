using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
namespace Capa_Datos.Inventario_DAO.TablasSql
{
    public class EspecificacionesTecnicas_D
    {
        Utilitarios uti = new Utilitarios(); DBHelper db = new DBHelper();
        public (Helper_E, List<EspecificacionesTecnicas_E>) ListarEspTecnicas(
            string condicion = "",
            Dictionary<string, object> parametros = null)
        {
            var lista = new List<EspecificacionesTecnicas_E>();
            var helper = new Helper_E();

            try
            {
                using (var cn = new SqlConnection(uti.cadSql))
                using (var cmd = cn.CreateCommand())
                {
                    var sb = new StringBuilder();

                    sb.AppendLine(@"
                                SELECT TOP 100
                                    x.ItemCode,
                                    pr.ItemName,
                                    x.Ruta,
                                    x.Nombre,
                                    x.OpCarga,
                                    x.TiempoCarga
                                FROM (
                                    SELECT
                                        e.ItemCode,
                                        e.Ruta,
                                        e.Nombre,
                                        e.OpCarga,
                                        e.TiempoCarga,
                                        ROW_NUMBER() OVER (
                                            PARTITION BY e.ItemCode
                                            ORDER BY e.TiempoCarga DESC
                                        ) AS rn
                                    FROM EspecificacionesTecnicas e
                                ) x
                                LEFT JOIN Productos pr ON pr.ItemCode = x.ItemCode
                                WHERE x.rn = 1
                            ");

                    if (!string.IsNullOrWhiteSpace(condicion))
                    {
                        sb.AppendLine(condicion);
                    }

                    sb.AppendLine("ORDER BY x.TiempoCarga DESC");

                    cmd.CommandText = sb.ToString();
                    cmd.CommandType = CommandType.Text;

                    if (parametros != null)
                    {
                        foreach (var prm in parametros)
                        {
                            cmd.Parameters.Add(
                                new SqlParameter(prm.Key, prm.Value ?? DBNull.Value)
                            );
                        }
                    }

                    cn.Open();

                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            var esp = new EspecificacionesTecnicas_E
                            {
                                ItemCode = dr.GetString(0),
                                ItemName = dr.IsDBNull(1) ? null : dr.GetString(1),
                                Ruta = dr.IsDBNull(2) ? null : dr.GetString(2),
                                Nombre = dr.IsDBNull(3) ? null : dr.GetString(3),
                                OpCarga = dr.IsDBNull(4) ? null : dr.GetString(4),
                                TiempoCarga = dr.GetDateTime(5)
                            };

                            if (!string.IsNullOrWhiteSpace(esp.Ruta))
                            {
                                var rutaFisica = Path.Combine(
                                    uti.directorioFileServer.TrimEnd('/', '\\'),
                                    esp.Ruta
                                );

                                esp.RutaArchivo = File.Exists(rutaFisica)
                                    ? rutaFisica
                                    : null;
                            }

                            lista.Add(esp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "EspecificacionesTecnicas_D.ListarEspTecnicas");

                helper.Titulo = "Error";
                helper.Icono = "error";
                helper.Mensajes.Add("Ocurrió un error al listar especificaciones técnicas.");
                helper.Mensajes.Add("Comuníquese con el área de Sistemas.");
            }

            return (helper, lista);
        }
        public int AgregarEspTecnica(EspecificacionesTecnicas_E obj)
        {
            int status = -1;
            string query = "";
            string rutaDirectorio = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), obj.ItemCode);
            if (BuscarEspTecnica(obj.ItemCode) == null)
            {
                query = "insert into EspecificacionesTecnicas values(@ItemCode,@Ruta,@Nombre,@OpCarga,@TiempoCarga);";
            }
            else
            {
                query = "update EspecificacionesTecnicas set Ruta=@Ruta,Nombre=@Nombre,OpCarga=@OpCarga,TiempoCarga=@TiempoCarga where ItemCode=@ItemCode;";
            } 
            obj.Ruta = Path.Combine(obj.ItemCode, obj.Nombre + ".pdf");
            SqlConnection cn = new SqlConnection(uti.cadSql);
            try
            {
                cn.Open();
                SqlTransaction tran = cn.BeginTransaction();
                try
                {
                    var datosProd = new Productos_D();
                    string condicion = " AND ItemCode = @ItemCode";
                    var parametros = new Dictionary<string, object> { { "@ItemCode", obj.ItemCode } };

                    var resultadoProductos = datosProd.ObtenerProductos(condicion, parametros);
                    var producto = resultadoProductos.Item2.DefaultIfEmpty(new Productos_E()).First();

                    if (producto == null || string.IsNullOrEmpty(producto.ItemName))
                        datosProd.AgregarArticulo(new Productos_E() { ItemCode = obj.ItemCode, ItemName = obj.ItemName, FirmCode = obj.FirmCode });
                    SqlCommand cmd = new SqlCommand(query, cn, tran);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@ItemCode", obj.ItemCode);
                    cmd.Parameters.AddWithValue("@Ruta", obj.Ruta);
                    cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@OpCarga", obj.OpCarga);
                    cmd.Parameters.AddWithValue("@TiempoCarga", DateTime.Now);
                    cmd.ExecuteNonQuery();
                    if (!Directory.Exists(rutaDirectorio))
                        Directory.CreateDirectory(rutaDirectorio);
                    string rutaCompletaArchivo = Path.Combine(rutaDirectorio, "ET.pdf");
                    obj.ArchivoPdf.SaveAs(rutaCompletaArchivo);
                    tran.Commit();
                    status = 1;
                    cn.Close();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    cn.Close();
                    throw new Exception("Error en registro: " + e.Message);
                }
            }
            catch (Exception e2) { cn.Close(); status = 0; throw new Exception("Error en conexion: " + e2.Message); }
            return status;
        }
        public EspecificacionesTecnicas_E BuscarEspTecnica(string ItemCode)
        {
            EspecificacionesTecnicas_E et = null;
            string query = "select * from EspecificacionesTecnicas where ItemCode=@ItemCode";
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ItemCode", ItemCode);
                SqlDataReader dr = cmd.ExecuteReader();
                et = new EspecificacionesTecnicas_E();
                if (dr.Read())
                {
                    if (!dr.IsDBNull(0)) et.ItemCode = dr.GetString(0);
                    if (!dr.IsDBNull(1)) et.Ruta = dr.GetString(1);
                    if (!dr.IsDBNull(2)) et.Nombre = dr.GetString(2);
                    if (!dr.IsDBNull(3)) et.OpCarga = dr.GetString(3);
                    if (!dr.IsDBNull(4)) et.TiempoCarga = dr.GetDateTime(4);
                    var rutaArchivo = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), et.Ruta);
                    et.RutaArchivo = File.Exists(rutaArchivo) ? rutaArchivo : null;
                    dr.Close();
                }
                else
                { et = null; }
                cn.Close();
            }
            catch { cn.Close(); et = null; }
            return et;
        }
        public Helper_E EliminarEspTecnicas(string ItemCode)
        {
            Helper_E helper = new Helper_E();

            try
            {
                string query = "DELETE FROM EspecificacionesTecnicas WHERE ItemCode = @ItemCode";
                using (SqlConnection cn = db.SqlConexion())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@ItemCode", ItemCode);
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            helper.Estado = true;
                            helper.Mensaje = "Esp Tecnica eliminado correctamente.";
                        }
                        else
                        {
                            helper.Estado = false;
                            helper.Mensaje = "No se encontró un Esp Tecnica con el Itemcode proporcionado.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                helper.Estado = false;
                helper.Mensaje = "Error al eliminar la Esp Tecnica: " + ex.Message;
            }

            return helper;
        }

        public bool ExisteEspTec(string itemCode)
        {
            using (var cn = new SqlConnection(uti.cadSql))
            {
                cn.Open();
                string query = @"SELECT COUNT(1) 
                         FROM EspecificacionesTecnicas 
                         WHERE ItemCode = @ItemCode";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // true si existe, false si no
                }
            }
        }

        public List<EspecificacionesTecnicas_E> ListarReporteEspTecnica(string opCarga, DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<EspecificacionesTecnicas_E>();

            // 1. TRUCO PARA FECHAS (EXACTO A TU EJEMPLO DE PROTOCOLOS):
            // Inicio a las 00:00:00
            DateTime desde = fechaInicio.Date;
            // Fin al día siguiente a las 00:00:00 (para abarcar todo el día seleccionado)
            DateTime hasta = fechaFin.Date.AddDays(1);

            try
            {
                using (var cn = new SqlConnection(uti.cadSql))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        var sb = new StringBuilder();

                        // 2. CONSULTA SQL ADAPTADA A ESPECIFICACIONES TECNICAS:
                        // Seleccionamos las columnas equivalentes
                        sb.AppendLine("SELECT E.ItemCode, pr.ItemName, E.Ruta, E.Nombre, E.OpCarga, E.TiempoCarga");
                        sb.AppendLine("FROM EspecificacionesTecnicas E");
                        sb.AppendLine("LEFT JOIN Productos pr ON E.ItemCode = pr.ItemCode");

                        // Rango de fechas OBLIGATORIO (Igual que en Protocolos)
                        sb.AppendLine("WHERE E.TiempoCarga >= @Desde AND E.TiempoCarga < @Hasta");

                        // 3. FILTRO DE USUARIO (SOLO SI EL REPORTE ES PARA UN USUARIO ESPECÍFICO)
                        if (!string.IsNullOrWhiteSpace(opCarga))
                        {
                            sb.AppendLine("AND E.OpCarga LIKE @OpCarga");
                        }

                        sb.AppendLine("ORDER BY E.TiempoCarga DESC");

                        cmd.CommandText = sb.ToString();

                        // 4. PARÁMETROS:
                        // Aquí pasamos las fechas DIRECTAS porque NO son null
                        cmd.Parameters.AddWithValue("@Desde", desde);
                        cmd.Parameters.AddWithValue("@Hasta", hasta);

                        if (!string.IsNullOrWhiteSpace(opCarga))
                        {
                            cmd.Parameters.AddWithValue("@OpCarga", "%" + opCarga.Trim() + "%");
                        }

                        cn.Open();

                        using (var pr = cmd.ExecuteReader())
                        {
                            while (pr.Read())
                            {
                                var item = new EspecificacionesTecnicas_E();

                                // Mapeo exacto de columnas
                                if (!pr.IsDBNull(0)) item.ItemCode = pr.GetString(0);
                                if (!pr.IsDBNull(1)) item.ItemName = pr.GetString(1);
                                if (!pr.IsDBNull(2)) item.Ruta = pr.GetString(2);
                                if (!pr.IsDBNull(3)) item.Nombre = pr.GetString(3);
                                if (!pr.IsDBNull(4)) item.OpCarga = pr.GetString(4);
                                if (!pr.IsDBNull(5)) item.TiempoCarga = pr.GetDateTime(5);

                                // Armar ruta del archivo
                                if (!string.IsNullOrEmpty(item.Ruta))
                                {
                                    string baseDir = uti.directorioFileServer ?? "";
                                    item.RutaArchivo = Path.Combine(baseDir.TrimEnd('/', '\\'), item.Ruta);
                                }
                                else
                                {
                                    item.RutaArchivo = "";
                                }

                                lista.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Error en Especificaciones_D - ListarReporteEspTecnica");
                throw;
            }

            return lista;
        }
    }
}