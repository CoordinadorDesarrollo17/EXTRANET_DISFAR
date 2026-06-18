using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Text;
namespace Capa_Datos.Inventario_DAO.TablasSql
{
    public class LotesRegistroSanitario_D
    {
        private readonly Utilitarios uti = new Utilitarios();
        DBHelper db = new DBHelper();
        public void AgregarLote(LotesRegistroSanitario_E obj)
        {
            string query = "insert into LotesRegistroSanitario values(@ItemCode,@DistNumber,@MnfSerial,@ExpDate)";
            try
            {
                db.MysqlExecuteNonQueryTrxNoSp(query, new List<string>() { "@ItemCode", "@DistNumber", "@MnfSerial", "@ExpDate" }
                    , obj.ItemCode, obj.DistNumber, obj.MnfSerial, obj.ExpDate);
            }
            catch { }
        }
        public (Helper_E, List<LotesRegistroSanitario_E>) ListarLotesRegistroSanitario(string condicion = "", Dictionary<string, object> parametros = null)
        {
            var lista = new List<LotesRegistroSanitario_E>();
            Helper_E _helper = new Helper_E();

            var top = string.IsNullOrEmpty(condicion) ? "TOP 100" : string.Empty;

            try
            {
                using (var cn = new SqlConnection(uti.cadSql))
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = cn;

                    var sb = new StringBuilder();
                    sb.AppendLine($"SELECT {top} Pr.ItemCode, Pr.ItemName, Lt.DistNumber, Lt.MnfSerial, CONVERT(varchar, Lt.ExpDate, 103)");
                    sb.AppendLine("FROM Productos Pr");
                    sb.AppendLine("LEFT JOIN LotesRegistroSanitario Lt ON Lt.ItemCode = Pr.ItemCode");
                    sb.AppendLine("WHERE 1=1");

                    if (!string.IsNullOrWhiteSpace(condicion))
                    {
                        sb.AppendLine(condicion.Trim());
                    }

                    sb.AppendLine("ORDER BY Pr.ItemCode");

                    cmd.CommandText = sb.ToString();

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
                        var oeetD = new EspecificacionesTecnicas_D();
                        var oeptD = new Protocolos_D();
                        var oersD = new RegistrosSanitarios_D();

                        while (dr.Read())
                        {
                            var o = new LotesRegistroSanitario_E();

                            if (!dr.IsDBNull(0)) o.ItemCode = dr.GetString(0);
                            if (!dr.IsDBNull(1)) o.ItemName = dr.GetString(1);
                            if (!dr.IsDBNull(2)) o.DistNumber = dr.GetString(2);
                            if (!dr.IsDBNull(3)) o.MnfSerial = dr.GetString(3);
                            if (!dr.IsDBNull(4)) o.ExpDate = dr.GetString(4);
                            o.existeArchivoEt(oeetD.BuscarEspTecnica(o.ItemCode), uti.directorioFileServer);
                            o.existeArchivoPt(oeptD.BuscarProtocolo(o.ItemCode, o.DistNumber), uti.directorioFileServer);
                            o.existeArchivoRs(oersD.BuscarRegSanitario(o.MnfSerial), uti.directorioFileServer);

                            lista.Add(o);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.RegistrarError(ex, "Error inesperado en ListarLotesRegistroSanitario()");

                _helper.Titulo = "Error";
                _helper.Mensajes.Add("Ocurrió un error al listar lotes de registro sanitario.");
                _helper.Mensajes.Add("Por favor, comuníquese con el área de Sistemas para más información.");
                _helper.Icono = "error";
            }

            return (_helper, lista);
        }


        public LotesRegistroSanitario_E BuscarLotesRegistroSanitario(string ItemCode, string DistNumber)
        {
            var o = new LotesRegistroSanitario_E();
            string query = "SELECT ItemCode, DistNumber, MnfSerial, CONVERT(varchar, ExpDate, 103) FROM LotesRegistroSanitario WHERE ItemCode=@ItemCode and DistNumber=@DistNumber";
            SqlConnection cn = db.SqlConexion();
            try
            {
                cn.Open();
                SqlCommand cmd = new SqlCommand(query, cn);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@ItemCode", ItemCode);
                cmd.Parameters.AddWithValue("@DistNumber", DistNumber);
                SqlDataReader dr = cmd.ExecuteReader();
                dr.Read();
                if (!dr.IsDBNull(0)) { o.ItemCode = dr.GetString(0); }
                if (!dr.IsDBNull(1)) { o.DistNumber = dr.GetString(1); }
                if (!dr.IsDBNull(2)) { o.MnfSerial = dr.GetString(2); }
                if (!dr.IsDBNull(3)) { o.ExpDate = dr.GetString(3); }
                dr.Close();
                cn.Close();
            }
            catch { cn.Close(); o = null; }
            return o;
        }
    }
}
