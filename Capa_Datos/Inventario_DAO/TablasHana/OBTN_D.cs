using Capa_Entidad.Inventario_ENT.TablasHana;
using System.Collections.Generic;
using Sap.Data.Hana;
namespace Capa_Datos.Inventario_DAO.TablasHana
{
    public class OBTN_D
    {
        DBHelper db = new DBHelper(); 
        Utilitarios uti = new Utilitarios();
        public List<OBTN_E> ListarLotesArticulo(string ItemCode)
        {
            List<OBTN_E> lista = new List<OBTN_E>();
            string query = "select \"ItemCode\",\"DistNumber\",\"MnfSerial\",\"ExpDate\" from " + uti.schemaHana + "obtn where \"ItemCode\"='" + ItemCode + "' order by \"DistNumber\"";
            try
            {
                HanaDataReader hdr = db.HanaExecuteReaderNoSp(query);
                while (hdr.Read())
                {
                    OBTN_E o = new OBTN_E();
                    if (!hdr.IsDBNull(0)) { o.ItemCode = hdr.GetString(0); }
                    if (!hdr.IsDBNull(1)) { o.DistNumber = hdr.GetString(1); }
                    if (!hdr.IsDBNull(2)) { o.MnfSerial = hdr.GetString(2); }
                    if (!hdr.IsDBNull(3)) { o.ExpDate = hdr.GetDateTime(3); }
                    lista.Add(o);
                }
                hdr.Close();
            }
            catch { }
            return lista;
        }
        public string SelectLotesArticulo(string ItemCode)
        {
            string lista = "<option value=''>Elija Lote</option>";
            try
            {
                foreach (OBTN_E o in ListarLotesArticulo(ItemCode))
                {
                    lista += "<option value=\"" + o.DistNumber + "\" data-mnfserial=\"" +
                        o.MnfSerial + "\" data-expdate=\"" + o.ExpDate.ToString("yyyy-MM-dd") + "\">" + o.DistNumber +
                        "</option>";
                }
            }
            catch { }
            return lista;
        }
        public List<OBTN_E> ObtenerRegistrosSanitariosExistentes(OBTN_E filtros)
        {
            var lista = new List<OBTN_E>();
            var query = $@"SELECT DISTINCT(""MnfSerial"") FROM {uti.schemaHana}obtn 
                   WHERE ""MnfSerial"" IS NOT NULL AND ""MnfSerial"" != ''";

            if (filtros != null && !string.IsNullOrWhiteSpace(filtros.MnfSerial))
            {
                var mnf = filtros.MnfSerial.Replace("'", "''");
                query += $@" AND UPPER(""MnfSerial"") LIKE '%{mnf.ToUpper()}%'";
            }

            query += " ORDER BY 1";

            try
            {
                HanaDataReader hdr = db.HanaExecuteReaderNoSp(query);
                while (hdr.Read())
                {
                    OBTN_E o = new OBTN_E();
                    if (!hdr.IsDBNull(0)) o.MnfSerial = hdr.GetString(0);
                    lista.Add(o);
                }
                hdr.Close();
            }
            catch { }

            return lista;
        }

    }
}
