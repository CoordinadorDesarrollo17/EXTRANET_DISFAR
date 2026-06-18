using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace Capa_Datos.Inventario_DAO.TablasSql
{
    public class RegistrosSanitarios_D
    {
        DBHelper db = new DBHelper();
        Utilitarios uti = new Utilitarios();

        public int AgregarRegSanitario(RegistrosSanitarios_E obj)
        {
            int status = -1;
            string query = "";

            // Definimos la ruta física donde se guardará
            string rutaDirectorio = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), "RegistrosSanitarios");

            // Definimos la ruta relativa para la BD
            obj.Ruta = Path.Combine("RegistrosSanitarios", obj.MnfSerial + ".pdf");

            // Validamos si existe para decidir INSERT o UPDATE
            // Nota: Es mejor especificar las columnas en el INSERT para evitar errores si la tabla cambia
            if (!ExisteSerial(obj.MnfSerial))
            {
                query = "INSERT INTO RegistrosSanitarios (MnfSerial, Ruta, OpCarga, TiempoCarga) VALUES (@MnfSerial, @Ruta, @OpCarga, @TiempoCarga);";
            }
            else
            {
                query = "UPDATE RegistrosSanitarios SET Ruta=@Ruta, OpCarga=@OpCarga, TiempoCarga=@TiempoCarga WHERE MnfSerial=@MnfSerial;";
            }

            // Usamos 'using' para asegurar que la conexión se cierre siempre
            using (SqlConnection cn = new SqlConnection(uti.cadSql))
            {
                try
                {
                    cn.Open();
                    using (SqlTransaction tran = cn.BeginTransaction())
                    {
                        try
                        {
                            using (SqlCommand cmd = new SqlCommand(query, cn, tran))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@MnfSerial", obj.MnfSerial);
                                cmd.Parameters.AddWithValue("@Ruta", obj.Ruta);
                                cmd.Parameters.AddWithValue("@OpCarga", obj.OpCarga);
                                cmd.Parameters.AddWithValue("@TiempoCarga", DateTime.Now);

                                cmd.ExecuteNonQuery();
                            }

                            // Manejo del Archivo
                            if (!Directory.Exists(rutaDirectorio))
                                Directory.CreateDirectory(rutaDirectorio);

                            string rutaCompletaArchivo = Path.Combine(rutaDirectorio, obj.MnfSerial + ".pdf");

                            // Guardamos el archivo físico
                            if (obj.ArchivoPdf != null) // Validación extra por seguridad
                            {
                                obj.ArchivoPdf.SaveAs(rutaCompletaArchivo);
                            }

                            tran.Commit();
                            status = 1;
                        }
                        catch (Exception exInt)
                        {
                            tran.Rollback();
                            // Aquí podrías agregar un log de error
                            throw new Exception("Error al procesar la transacción: " + exInt.Message);
                        }
                    }
                }
                catch (Exception exCon)
                {
                    status = 0;
                    throw new Exception("Error de conexión o base de datos: " + exCon.Message);
                }
            }
            return status;
        }

        public RegistrosSanitarios_E BuscarRegSanitario(string MnfSerial)
        {
            RegistrosSanitarios_E rs = null;
            string query = "SELECT * FROM RegistrosSanitarios WHERE MnfSerial = @MnfSerial";

            using (SqlConnection cn = db.SqlConexion())
            {
                try
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@MnfSerial", MnfSerial);

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                            {
                                rs = new RegistrosSanitarios_E();
                                if (!dr.IsDBNull(0)) rs.MnfSerial = dr.GetString(0);
                                if (!dr.IsDBNull(1)) rs.Ruta = dr.GetString(1);
                                if (!dr.IsDBNull(2)) rs.OpCarga = dr.GetString(2);
                                if (!dr.IsDBNull(3)) rs.TiempoCarga = dr.GetDateTime(3);

                                // Lógica de ruta archivo
                                var rutaArchivo = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), rs.Ruta);
                                rs.RutaArchivo = File.Exists(rutaArchivo) ? rutaArchivo : null;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    rs = null;
                }
            }
            return rs;
        }

        public (Helper_E, List<RegistrosSanitarios_E>) ListarRegSanitarios(string condicion = "", Dictionary<string, object> parametros = null)
        {
            var lista = new List<RegistrosSanitarios_E>();
var helper = new Helper_E();

    try
    {
        using (var cn = new SqlConnection(uti.cadSql))
    using (var cmd = cn.CreateCommand())
        {
        var sb = new StringBuilder();
     
            // ✅ SOLUCIÓN: Subconsulta con ROW_NUMBER para evitar duplicados
  sb.AppendLine(@"
    SELECT TOP 100 
   x.MnfSerial, 
    x.Ruta, 
        x.OpCarga, 
    x.TiempoCarga,
  y.ItemCode,
        y.ItemName
    FROM (
   SELECT rs.MnfSerial, rs.Ruta, rs.OpCarga, rs.TiempoCarga,
            ROW_NUMBER() OVER (PARTITION BY rs.MnfSerial ORDER BY rs.TiempoCarga DESC) AS rn
  FROM RegistrosSanitarios rs
    ) x
 LEFT JOIN (
        SELECT 
            lrs.MnfSerial,
        lrs.ItemCode,
   pr.ItemName,
            ROW_NUMBER() OVER (PARTITION BY lrs.MnfSerial ORDER BY lrs.ItemCode) AS rn_lote
      FROM LotesRegistroSanitario lrs
        LEFT JOIN Productos pr ON pr.ItemCode = lrs.ItemCode
    ) y ON y.MnfSerial = x.MnfSerial AND y.rn_lote = 1
    WHERE x.rn = 1 ");

            if (!string.IsNullOrWhiteSpace(condicion))
   {
          sb.AppendLine(condicion);
 }
     sb.AppendLine("ORDER BY x.TiempoCarga DESC");

cmd.CommandText = sb.ToString();

         if (parametros != null)
        {
            foreach (var prm in parametros)
         {
             cmd.Parameters.Add(new SqlParameter(prm.Key, prm.Value ?? DBNull.Value));
          }
          }

          cn.Open();
      using (var dr = cmd.ExecuteReader())
     {
    while (dr.Read())
     {
         var reg = new RegistrosSanitarios_E
             {
    MnfSerial = dr.GetString(0),
          Ruta = dr.IsDBNull(1) ? null : dr.GetString(1),
         OpCarga = dr.IsDBNull(2) ? null : dr.GetString(2),
  TiempoCarga = dr.GetDateTime(3),
  ItemCode = dr.IsDBNull(4) ? null : dr.GetString(4),
 ItemName = dr.IsDBNull(5) ? null : dr.GetString(5)
        };

        if (!string.IsNullOrWhiteSpace(reg.Ruta))
   {
             var rutaFisica = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), reg.Ruta);
         reg.RutaArchivo = File.Exists(rutaFisica) ? rutaFisica : null;
     }
    lista.Add(reg);
     }
     }
        }
    }
    catch (Exception ex)
    {
        helper.Titulo = "Error";
        helper.Icono = "error";
        helper.Mensajes.Add("Ocurrió un error al listar los registros sanitarios: " + ex.Message);
    }

    return (helper, lista);
}

        public Helper_E EliminarRegSanitario(string MnfSerial)
        {
            Helper_E helper = new Helper_E();

            try
            {
                string query = "DELETE FROM RegistrosSanitarios WHERE MnfSerial = @MnfSerial";
                using (SqlConnection cn = db.SqlConexion())
                {
                    cn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, cn))
                    {
                        cmd.Parameters.AddWithValue("@MnfSerial", MnfSerial);
                        int filasAfectadas = cmd.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            helper.Estado = true;
                            helper.Mensaje = "Registro sanitario eliminado correctamente.";
                        }
                        else
                        {
                            helper.Estado = false;
                            helper.Mensaje = "No se encontró el registro.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                helper.Estado = false;
                helper.Mensaje = "Error al eliminar: " + ex.Message;
            }

            return helper;
        }

        public bool ExisteSerial(string mnfserial)
        {
            using (var cn = new SqlConnection(uti.cadSql))
            {
                cn.Open();
                string query = "SELECT COUNT(1) FROM RegistrosSanitarios WHERE MnfSerial = @MnfSerial";
                using (var cmd = new SqlCommand(query, cn))
                {
                    cmd.Parameters.AddWithValue("@MnfSerial", mnfserial);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        public List<RegistrosSanitarios_E> ListarReporteRegSanitario(string opCarga, DateTime fechaInicio, DateTime fechaFin)
        {
            var lista = new List<RegistrosSanitarios_E>();
    DateTime desde = fechaInicio.Date;
 DateTime hasta = fechaFin.Date.AddDays(1);

    try
    {
        using (var cn = new SqlConnection(uti.cadSql))
        using (var cmd = new SqlCommand())
  {
        cmd.Connection = cn;
   var sb = new StringBuilder();

    // ✅ SOLUCIÓN: Usar ROW_NUMBER para evitar duplicados en el reporte
      sb.AppendLine(@"
   SELECT 
    rs.MnfSerial, 
  rs.Ruta, 
      rs.OpCarga, 
  rs.TiempoCarga, 
   y.ItemCode, 
   y.ItemName
  FROM RegistrosSanitarios rs
      LEFT JOIN (
    SELECT 
   lrs.MnfSerial,
lrs.ItemCode,
    pr.ItemName,
        ROW_NUMBER() OVER (PARTITION BY lrs.MnfSerial ORDER BY lrs.ItemCode) AS rn_lote
       FROM LotesRegistroSanitario lrs
     LEFT JOIN Productos pr ON pr.ItemCode = lrs.ItemCode
        ) y ON y.MnfSerial = rs.MnfSerial AND y.rn_lote = 1
   WHERE rs.TiempoCarga >= @Desde AND rs.TiempoCarga < @Hasta");

            if (!string.IsNullOrWhiteSpace(opCarga))
   {
     sb.AppendLine("AND rs.OpCarga LIKE @OpCarga");
}

sb.AppendLine("ORDER BY rs.TiempoCarga DESC");
      cmd.CommandText = sb.ToString();

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
        var item = new RegistrosSanitarios_E();
   if (!pr.IsDBNull(0)) item.MnfSerial = pr.GetString(0);
          if (!pr.IsDBNull(1)) item.Ruta = pr.GetString(1);
        if (!pr.IsDBNull(2)) item.OpCarga = pr.GetString(2);
         if (!pr.IsDBNull(3)) item.TiempoCarga = pr.GetDateTime(3);
         if (!pr.IsDBNull(4)) item.ItemCode = pr.GetString(4);
  if (!pr.IsDBNull(5)) item.ItemName = pr.GetString(5);

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
    catch (Exception ex)
    {
   throw new Exception("Error en reporte: " + ex.Message);
    }
    return lista;
}
    }
}