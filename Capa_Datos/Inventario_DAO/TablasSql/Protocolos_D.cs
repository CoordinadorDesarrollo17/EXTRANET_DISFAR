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
    public class Protocolos_D
    {
        Utilitarios uti = new Utilitarios();
        DBHelper db = new DBHelper();

        public (Helper_E, List<Protocolos_E>) ListarProtocolos(string condicion = "", Dictionary<string, object> parametros = null)
        {
            var lista = new List<Protocolos_E>();
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
                                x.DistNumber,
                                x.Ruta,
                                x.OpCarga,
                                x.TiempoCarga
                            FROM (
                                SELECT 
                                    p.ItemCode,
                                    p.DistNumber,
                                    p.Ruta,
                                    p.OpCarga,
                                    p.TiempoCarga,
                                    ROW_NUMBER() OVER (
                                        PARTITION BY p.ItemCode, p.DistNumber
                                        ORDER BY p.TiempoCarga DESC
                                    ) AS rn
                                FROM Protocolos p
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

                    // Parámetros seguros
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
                            string FixEncoding(string texto)
                            {
                                if (string.IsNullOrEmpty(texto)) return texto;

                                // AQUÍ ESTÁ EL TRUCO DE SEGURIDAD:
                                // Solo intentamos arreglarlo si detectamos explícitamente el error de la ñ.
                                // Si el texto dice "José", no entrará aquí y no se romperá.
                                if (texto.Contains("├▒"))
                                {
                                    try
                                    {
                                        // 437 es el código para interpretar esos símbolos de DOS
                                        byte[] bytes = System.Text.Encoding.GetEncoding(437).GetBytes(texto);
                                        return System.Text.Encoding.UTF8.GetString(bytes);
                                    }
                                    catch
                                    {
                                        return texto; // Si falla la conversión, devuelve el original
                                    }
                                }

                                // Si no tiene el error, devolvemos el texto tal cual
                                return texto;
                            }
                            var protocolo = new Protocolos_E
                            {
                                ItemCode = dr.GetString(0),
                                ItemName = dr.IsDBNull(1) ? null : dr.GetString(1),
                                DistNumber = dr.GetString(2),
                                Ruta = dr.IsDBNull(3) ? null : dr.GetString(3),
                                OpCarga = dr.IsDBNull(4) ? null : FixEncoding(dr.GetString(4)),
                                TiempoCarga = dr.GetDateTime(5)
                            };

                            // Validación física del archivo
                            if (!string.IsNullOrWhiteSpace(protocolo.Ruta))
                            {
                                var rutaFisica = Path.Combine(
                                    uti.directorioFileServer.TrimEnd('/', '\\'),
                                    protocolo.Ruta
                                );

                                protocolo.RutaArchivo = File.Exists(rutaFisica)
                                    ? rutaFisica
                                    : null;
                            }

                            lista.Add(protocolo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Protocolos_D.ListarProtocolos");

                helper.Titulo = "Error";
                helper.Icono = "error";
                helper.Mensajes.Add("Ocurrió un error al listar los protocolos.");
                helper.Mensajes.Add("Comuníquese con el área de Sistemas.");
            }

            return (helper, lista);
        }

        public int AgregarProtocolo(Protocolos_E obj)
        {
            int status = -1;
            string rutaDirectorio = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), obj.ItemCode);
            bool protocoloExiste = BuscarProtocolo(obj.ItemCode, obj.DistNumber) != null;
            var query = protocoloExiste
                ? "UPDATE Protocolos SET Ruta=@Ruta, OpCarga=@OpCarga, TiempoCarga=@TiempoCarga WHERE ItemCode=@ItemCode AND DistNumber=@DistNumber;"
                : "INSERT INTO Protocolos (ItemCode, DistNumber, Ruta, OpCarga, TiempoCarga) VALUES (@ItemCode, @DistNumber, @Ruta, @OpCarga, @TiempoCarga);";
            obj.Ruta = Path.Combine(obj.ItemCode, obj.DistNumber + ".pdf");
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
                    var datosLotesRS = new LotesRegistroSanitario_D();
                    var loteRS = datosLotesRS.BuscarLotesRegistroSanitario(obj.ItemCode, obj.DistNumber);
                    if (loteRS == null || string.IsNullOrEmpty(loteRS.DistNumber))
                        datosLotesRS.AgregarLote(new LotesRegistroSanitario_E() { ItemCode = obj.ItemCode, DistNumber = obj.DistNumber, MnfSerial = obj.MnfSerial, ExpDate = obj.ExpDate });
                    if (obj.ArchivoPdf != null)
                    {
                        SqlCommand cmd = new SqlCommand(query, cn, tran);
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@ItemCode", obj.ItemCode);
                        cmd.Parameters.AddWithValue("@DistNumber", obj.DistNumber);
                        cmd.Parameters.AddWithValue("@Ruta", obj.Ruta);
                        cmd.Parameters.AddWithValue("@OpCarga", obj.OpCarga);
                        cmd.Parameters.AddWithValue("@TiempoCarga", DateTime.Now);
                        cmd.ExecuteNonQuery();
                        if (!Directory.Exists(rutaDirectorio))
                            Directory.CreateDirectory(rutaDirectorio);
                        string rutaCompletaArchivo = Path.Combine(rutaDirectorio, obj.DistNumber + ".pdf");
                        obj.ArchivoPdf.SaveAs(rutaCompletaArchivo);
                    }
                    tran.Commit();
                    status = 1;
                    cn.Close();
                }
                catch (Exception e) { tran.Rollback(); cn.Close(); throw new Exception("Error en registro: " + e.Message); }
            }
            catch (Exception e2) { cn.Close(); status = 0; throw new Exception("Error en conexion: " + e2.Message); }
            return status;
        }
        public Protocolos_E BuscarProtocolo(string ItemCode, string DistNumber)
        {
            Protocolos_E protocolo = null;
            string query = "SELECT ItemCode, DistNumber, Ruta, OpCarga, TiempoCarga FROM Protocolos where ItemCode=@ItemCode and DistNumber=@DistNumber";
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ItemCode", ItemCode);
                cmd.Parameters.AddWithValue("@DistNumber", DistNumber);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    protocolo = new Protocolos_E();
                    if (!dr.IsDBNull(0)) protocolo.ItemCode = dr.GetString(0);
                    if (!dr.IsDBNull(1)) protocolo.DistNumber = dr.GetString(1);
                    if (!dr.IsDBNull(2)) protocolo.Ruta = dr.GetString(2);
                    if (!dr.IsDBNull(3)) protocolo.OpCarga = dr.GetString(3);
                    if (!dr.IsDBNull(4)) protocolo.TiempoCarga = dr.GetDateTime(4);
                    var rutaArchivo = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), protocolo.Ruta);
                    protocolo.RutaArchivo = File.Exists(rutaArchivo) ? rutaArchivo : null;
                    dr.Close();
                }
                cn.Close();
            }
            catch { cn.Close(); protocolo = null; }
            return protocolo;
        }

        public void EliminarProtocolo(string itemCode, string distNumber)
        {
            string query = "DELETE FROM Protocolos WHERE ItemCode = @ItemCode AND DistNumber = @DistNumber";
            using (SqlConnection cn = db.SqlConexion())
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.Parameters.AddWithValue("@ItemCode", itemCode);
                cmd.Parameters.AddWithValue("@DistNumber", distNumber);
                cmd.ExecuteNonQuery();
            }

            // Opcional: Eliminar el archivo del sistema de archivos si existe
            var protocolo = BuscarProtocolo(itemCode, distNumber);
            if (!string.IsNullOrEmpty(protocolo?.RutaArchivo) && File.Exists(protocolo.RutaArchivo))
            {
                File.Delete(protocolo.RutaArchivo);
            }
        }

        public bool ExisteLote(string itemCode, string lote)
        {
            using (var cn = new SqlConnection(uti.cadSql))
            {
                cn.Open();
                string query = @"SELECT COUNT(1) 
                         FROM Protocolos 
                         WHERE ItemCode = @ItemCode 
                           AND DistNumber = @DistNumber";

                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode);
                    cmd.Parameters.AddWithValue("@DistNumber", lote);

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // true si existe, false si no
                }
            }
        }


        public List<Protocolos_E> ListarReporteProtocolos(string opCarga, DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<Protocolos_E>();

            // 1. TRUCO PARA FECHAS INFALIBLE:
            // Aseguramos que inicio sea a las 00:00:00
            DateTime desde = fechaInicio.Date;
            // Para el fin, tomamos el día siguiente a las 00:00:00. 
            // Ejemplo: Si el usuario pide hasta el 17/01, buscaremos todo lo que sea MENOR al 18/01.
            DateTime hasta = fechaFin.Date.AddDays(1);

            try
            {
                using (var cn = new SqlConnection(uti.cadSql))
                {
                    using (var cmd = new SqlCommand())
                    {
                        cmd.Connection = cn;
                        var sb = new StringBuilder();

                        // 2. CONSULTA SQL MÁS SEGURA:
                        sb.AppendLine("SELECT p.ItemCode, pr.ItemName, p.DistNumber, p.Ruta, p.OpCarga, p.TiempoCarga");
                        sb.AppendLine("FROM Protocolos p");
                        sb.AppendLine("LEFT JOIN Productos pr ON p.ItemCode = pr.ItemCode");

                        // Usamos >= y < en lugar de BETWEEN para evitar problemas de horas
                        sb.AppendLine("WHERE p.TiempoCarga >= @Desde AND p.TiempoCarga < @Hasta");

                        // 3. FILTRO DE USUARIO (SOLO SI ESCRIBIÓ ALGO):
                        // Limpiamos espacios en blanco por si acaso
                        if (!string.IsNullOrWhiteSpace(opCarga))
                        {
                            sb.AppendLine("AND p.OpCarga LIKE @OpCarga");
                        }

                        sb.AppendLine("ORDER BY p.TiempoCarga DESC");

                        cmd.CommandText = sb.ToString();

                        // 4. PARÁMETROS:
                        cmd.Parameters.AddWithValue("@Desde", desde);
                        cmd.Parameters.AddWithValue("@Hasta", hasta);

                        if (!string.IsNullOrWhiteSpace(opCarga))
                        {
                            cmd.Parameters.AddWithValue("@OpCarga", "%" + opCarga.Trim() + "%");
                        }

                        cn.Open();

                        // Agregamos un chequeo rápido (opcional para debug)
                        // Console.WriteLine(cmd.CommandText); 

                        using (var pr = cmd.ExecuteReader())
                        {
                            while (pr.Read())
                            {
                                var protocolo = new Protocolos_E();

                                if (!pr.IsDBNull(0)) protocolo.ItemCode = pr.GetString(0);
                                if (!pr.IsDBNull(1)) protocolo.ItemName = pr.GetString(1);
                                if (!pr.IsDBNull(2)) protocolo.DistNumber = pr.GetString(2);
                                if (!pr.IsDBNull(3)) protocolo.Ruta = pr.GetString(3);
                                if (!pr.IsDBNull(4)) protocolo.OpCarga = pr.GetString(4);
                                if (!pr.IsDBNull(5)) protocolo.TiempoCarga = pr.GetDateTime(5);

                                // Armar ruta del archivo
                                if (!string.IsNullOrEmpty(protocolo.Ruta))
                                {
                                    // Aseguramos que uti.directorioFileServer tenga valor, si no, usa vacío
                                    string baseDir = uti.directorioFileServer ?? "";
                                    protocolo.RutaArchivo = Path.Combine(baseDir.TrimEnd('/', '\\'), protocolo.Ruta);
                                }
                                else
                                {
                                    protocolo.RutaArchivo = "";
                                }

                                lista.Add(protocolo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Error en Protocolos_D - ListarReporteProtocolos");
                throw;
            }

            return lista;
        }
    }
}