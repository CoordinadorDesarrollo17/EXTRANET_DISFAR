using Capa_Entidad.Seguridad_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
namespace Capa_Datos.Seguridad_DAO.TablasSql
{
    public class Usuarios_D
    {
        DBHelper db = new DBHelper();
        private readonly Utilitarios uti = new Utilitarios();
        public Usuario_E BuscarUsuarioLogueo(Usuario_E user)
        {
            Usuario_E obj = null;
            var sbQuery = new StringBuilder();
            sbQuery.AppendLine("SELECT USU.Id, USU.Usuario, USU.Nombre, USU.TipoUsuarioId, TP.Tipo");
            sbQuery.AppendLine("FROM Usuarios USU");
            sbQuery.AppendLine("LEFT JOIN TipoUsuario TP on TP.Id = USU.TipoUsuarioId ");
            sbQuery.AppendLine("WHERE Usuario = @Usuario ");
            sbQuery.AppendLine("AND CONVERT(VARCHAR(MAX), DECRYPTBYPASSPHRASE('C0b3f@rSAC', Contrasena)) = @Contrasena ");
            var query = sbQuery.ToString();
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.Text;
                // Agregar parámetros de manera segura
                cmd.Parameters.AddWithValue("@Usuario", user.Usuario);
                cmd.Parameters.AddWithValue("@Contrasena", user.Contrasena);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read()) // Solo si encuentra un usuario válido
                {
                    obj = new Usuario_E();
                    if (!dr.IsDBNull(0)) obj.Id = dr.GetInt32(0);
                    if (!dr.IsDBNull(1)) obj.Usuario = dr.GetString(1);
                    if (!dr.IsDBNull(2)) obj.Nombre = dr.GetString(2);
                    if (!dr.IsDBNull(3)) obj.TipoUsuarioId = dr.GetInt32(3);
                    if (!dr.IsDBNull(4)) obj.NombreTipoUsuario = dr.GetString(4);
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Usuarios_D - BuscarUsuarioLogueo");
                throw new Exception("Usuario o contraseña no válidos");
            }
            finally
            {
                cn.Close();
            }
            return obj;
        }
        public List<Usuario_E> ListarUsuarios(Usuario_E filtro)
        {
            var lista = new List<Usuario_E>();
            StringBuilder fil = new StringBuilder();
            List<SqlParameter> parametros = new List<SqlParameter>();
            // Construcción segura de la consulta SQL con parámetros
            string queryBase = "SELECT Usuario, Nombre, TipoUsuarioId FROM Usuarios WHERE Usuario IS NOT NULL";
            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Usuario))
                {
                    fil.Append(" AND Usuario LIKE @Usuario");
                    parametros.Add(new SqlParameter("@Usuario", $"%{filtro.Usuario}%"));
                }
                if (!string.IsNullOrEmpty(filtro.Nombre))
                {
                    fil.Append(" AND Nombre LIKE @Nombre");
                    parametros.Add(new SqlParameter("@Nombre", $"%{filtro.Nombre}%"));
                }
                if (filtro.TipoUsuarioId > 0)
                {
                    fil.Append(" AND TipoUsuarioId = @TipoUsuarioId");
                    parametros.Add(new SqlParameter("@TipoUsuarioId", filtro.TipoUsuarioId));
                }
            }
            string query = $"{queryBase} {fil} ORDER BY Nombre";
            try
            {
                using (SqlConnection cnx = new SqlConnection(uti.cadSql))
                {
                    cnx.Open();
                    using (SqlCommand cmd = new SqlCommand(query, cnx))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddRange(parametros.ToArray());
                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var usu = new Usuario_E();
                                usu.Usuario = dr.IsDBNull(0) ? null : dr.GetString(0);
                                usu.Nombre = dr.IsDBNull(1) ? null : dr.GetString(1);
                                usu.TipoUsuarioId = dr.IsDBNull(2) ? 0 : dr.GetInt32(2);
                                lista.Add(usu);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en listarUsuarios: {ex.Message}");
                throw; // Propagar excepción para manejarla en niveles superiores
            }
            return lista;
        }
        public List<Usuario_E> ListarUsuariosTipo(Usuario_E filtro, string user)
        {
            List<Usuario_E> lista = new List<Usuario_E>();
            string fil = "";
            if (filtro != null)
            {
                if (filtro.Usuario != null) { fil += " and u.Usuario like '%" + filtro.Usuario + "%'"; }
                if (filtro.Nombre != null) { fil += " and u.Nombre like '%" + filtro.Nombre + "%'"; }
                if (filtro.NombreTipoUsuario != null) { fil += " and t.TipoUsuarioId like '%" + filtro.TipoUsuarioId + "%'"; }
            }
            string query = @"
                select u.Id, u.Usuario, u.Nombre, t.Tipo, u.TiempoCreacion
                from Usuarios u
                join TipoUsuario t on t.Id = u.TipoUsuarioId
                where Usuario is not null
                and not Usuario = '" + user + "' " + fil + @"
                order by Nombre";

            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query);
                while (dr.Read())
                {
                    Usuario_E o = new Usuario_E();
                    if (!dr.IsDBNull(0)) { o.Id = dr.GetInt32(0); }
                    if (!dr.IsDBNull(1)) { o.Usuario = dr.GetString(1); }
                    if (!dr.IsDBNull(2)) { o.Nombre = dr.GetString(2); }
                    if (!dr.IsDBNull(3)) { o.NombreTipoUsuario = dr.GetString(3); }
                    if (!dr.IsDBNull(4)) { o.TiempoCreacion = dr.GetDateTime(4); }
                    lista.Add(o);
                }
                dr.Close();
            }
            catch { }

            return lista;
        }
        public Usuario_E ObtenerDatosUsuario(int id)
        {
            Usuario_E obj = null;

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT u.Id, ");
            sb.Append("u.Usuario, ");
            sb.Append("u.Nombre, ");
            sb.Append("CONVERT(VARCHAR(MAX), DECRYPTBYPASSPHRASE('C0b3f@rSAC', u.Contrasena)), ");
            sb.Append("ti.Tipo ");
            sb.Append("FROM Usuarios u ");
            sb.Append("INNER JOIN TipoUsuario ti ON ti.Id = u.TipoUsuarioId ");
            sb.Append("LEFT JOIN UsuarioOperacion uo ON uo.TipoUsuarioId = u.TipoUsuarioId ");
            sb.Append("WHERE u.Id = @Id");

            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(sb.ToString(), cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    obj = new Usuario_E();
                    if (!dr.IsDBNull(0)) obj.Id = dr.GetInt32(0);
                    if (!dr.IsDBNull(1)) obj.Usuario = dr.GetString(1);
                    if (!dr.IsDBNull(2)) obj.Nombre = dr.GetString(2);
                    if (!dr.IsDBNull(3)) obj.Contrasena = dr.GetString(3);
                    if (!dr.IsDBNull(4)) obj.NombreTipoUsuario = dr.GetString(4);
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Usuarios_D - ObtenerDatosUsuario");
                throw new Exception("Ocurrió un error al obtener datos. Por favor, comunicarse con SISTEMAS.");
            }
            finally
            {
                cn.Close();
            }

            return obj;
        }

        public string RegistrarUsuario(Usuario_E obj)
        {
            string mensaje = "";
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("MANT_USUARIOS", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                // Parámetros del procedimiento almacenado
                cmd.Parameters.AddWithValue("@Accion", "INS");
                cmd.Parameters.AddWithValue("@Usuario", obj.Usuario);
                cmd.Parameters.AddWithValue("@Nombre", obj.Nombre);
                cmd.Parameters.AddWithValue("@Contrasena", obj.Contrasena);
                cmd.Parameters.AddWithValue("@TipoUsuarioId", obj.TipoUsuarioId);
                cmd.Parameters.AddWithValue("@OpCreacion", obj.OpCreacion);
                // Ejecutar el procedimiento y obtener el mensaje de respuesta
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    mensaje = dr["Mensaje"].ToString();
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Usuarios_D - ActualizarContrasena");
                throw new Exception("Ocurrió un error al actualizar la contraseña. Por favor, comuníquese con SISTEMAS.");
            }
            finally
            {
                cn.Close();
            }
            return mensaje;
        }
        public string ActualizarContrasena(Usuario_E obj)
        {
            string mensaje = "";
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand("MANT_USUARIOS", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                // Parámetros del procedimiento almacenado
                cmd.Parameters.AddWithValue("@Accion", "UPD");
                cmd.Parameters.AddWithValue("@Usuario", obj.Usuario);
                cmd.Parameters.AddWithValue("@NuevaContrasena", obj.NuevaContrasena);
                cmd.Parameters.AddWithValue("@Contrasena", obj.Contrasena);
                // Ejecutar el procedimiento y obtener el mensaje de respuesta
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    mensaje = dr["Mensaje"].ToString();
                }
                dr.Close();
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Usuarios_D - ActualizarContrasena");
                throw new Exception("Ocurrió un error al actualizar la contraseña. Por favor, comuníquese con SISTEMAS.");
            }
            finally
            {
                cn.Close();
            }
            return mensaje;
        }
        private string GenerarContraseña()
        {
            //revisado
            Random rdn = new Random();
            string caracteres = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890%$#@";/*todos los  caracteres contemplados*/
            int longitud = caracteres.Length;
            char letra;
            int longitudContrasenia = 8;
            string contraseniaAleatoria = string.Empty;
            for (int i = 0; i < longitudContrasenia; i++)
            {
                letra = caracteres[rdn.Next(longitud)];
                contraseniaAleatoria += letra.ToString();
            }
            return contraseniaAleatoria;
        }
        public string ReseteoContrasena(Usuario_E obj)
        {
            if (obj == null)
                throw new Exception("Usuario inválido para resetear.");
            //Revisado
            string contr = GenerarContraseña();
            string msj = "";
            string query = "UPDATE Usuarios SET Contrasena = (SELECT ENCRYPTBYPASSPHRASE('C0b3f@rSAC', @Contrasena)) where Id = @IdUsuario";
            try
            {
                db.MysqlExecuteNonQueryTrxNoSp(query, new List<string>() { "@Contrasena", "@IdUsuario" }, contr, obj.Id);
                obj.NuevaContrasena = contr;
            }
            catch { throw new Exception("Error al resetear"); }
            return msj;
        }

        public bool ValidarContrasenaActual(int idUsuario, string contrasenaIngresada)
        {
            string query = @"
        SELECT COUNT(*) 
        FROM Usuarios
        WHERE Id = @IdUsuario
          AND CONVERT(VARCHAR, DECRYPTBYPASSPHRASE('C0b3f@rSAC', Contrasena)) = @ContrasenaIngresada";

            var parametros = new List<SqlParameter>
            {
                new SqlParameter("@IdUsuario", idUsuario),
                new SqlParameter("@ContrasenaIngresada", contrasenaIngresada)
            };

            int resultado = 0;

            using (SqlConnection con = new SqlConnection(uti.cadSql))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddRange(parametros.ToArray());
                    con.Open();
                    resultado = (int)cmd.ExecuteScalar();
                }
            }

            return resultado == 1;
        }

    }
}
