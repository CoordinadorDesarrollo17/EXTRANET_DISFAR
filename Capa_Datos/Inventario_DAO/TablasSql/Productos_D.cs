using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
namespace Capa_Datos.Inventario_DAO.TablasSql
{
    public class Productos_D
    {
        DBHelper db = new DBHelper();
        private Utilitarios uti = new Utilitarios();
        public void AgregarArticulo(Productos_E obj)
        {
            string query = "INSERT INTO Productos VALUES (@ItemCode,@ItemName,@FirmCode, @EstadoSKU)";
            try
            {
                db.MysqlExecuteNonQueryTrxNoSp(query, new List<string>() { "@ItemCode", "@ItemName", "@FirmCode", "@EstadoSKU" }
                    , obj.ItemCode, obj.ItemName, obj.FirmCode, "01");
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Productos_D - agregarArticulo");
                throw; // Propagar excepción para manejarla en niveles superiores
            }
        }
        public (Helper_E, List<Productos_E>) ObtenerProductos(string condicion, Dictionary<string, object> parametros)
        {
            var lista = new List<Productos_E>();
            Helper_E _helper = new Helper_E();

            string queryBase = "SELECT ItemCode, ItemName, FirmCode, Estado_SKU FROM Productos WHERE ItemCode IS NOT NULL";
            string query = $"{queryBase} {condicion} ORDER BY ItemName";

            try
            {
                using (SqlConnection cnx = new SqlConnection(uti.cadSql))
                {
                    cnx.Open();
                    using (SqlCommand cmd = new SqlCommand(query, cnx))
                    {
                        if (parametros != null && parametros.Count > 0)
                        {
                            foreach (var p in parametros)
                            {
                                var paramName = p.Key.StartsWith("@") ? p.Key : "@" + p.Key;
                                cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                            }
                        }

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                var pr = new Productos_E();

                                if (!dr.IsDBNull(0)) pr.ItemCode = dr.GetString(0);
                                if (!dr.IsDBNull(1)) pr.ItemName = dr.GetString(1);
                                if (!dr.IsDBNull(2)) pr.FirmCode = dr.GetInt32(2);
                                if (!dr.IsDBNull(3)) pr.Estado_SKU = dr.GetString(3);

                                lista.Add(pr);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Productos_D - ObtenerProductos");

                _helper.Titulo = "Error";
                _helper.Mensajes.Add("Ocurrió un error al obtener productos.");
                _helper.Mensajes.Add("Por favor, contacte al área de Sistemas.");
                _helper.Icono = "error";
            }

            return (_helper, lista);
        }


    }
}
