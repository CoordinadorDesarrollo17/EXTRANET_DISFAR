using Capa_Entidad.Inventario_ENT.TablasHana;
using Sap.Data.Hana;
using System;
using System.Collections.Generic;
namespace Capa_Datos.Inventario_DAO.TablasHana
{
    public class OITM_D
    {
        Utilitarios uti = new Utilitarios();DBHelper db = new DBHelper();
        public List<OITM_E> ListarArticulos(OITM_E fil)
        {
            List<OITM_E> lista = new List<OITM_E>();
            string filtros = "";
            if (fil != null)
            {
                if ((fil.ItemCode != null && fil.ItemCode != "") && (fil.ItemName == null || fil.ItemName == "")) { filtros += " and upper(\"ItemCode\")like upper('%" + fil.ItemCode + "%')"; }
                if ((fil.ItemName != null && fil.ItemName != "") && (fil.ItemCode == null || fil.ItemCode == "")) { filtros += " and upper(\"ItemName\") like upper('%" + fil.ItemName + "%')"; }
                if ((fil.ItemCode != null && fil.ItemCode != "") && (fil.ItemName != null && fil.ItemName != "")) { filtros += "and (upper(\"ItemCode\")like upper('%" + fil.ItemCode + "%')or(upper(\"ItemName\") like upper('%" + fil.ItemName + "%')))"; }                
                if (fil.FirmCode > 0) { filtros += " and \"FirmCode\"=" + fil.FirmCode; }
            }
            string query = "SELECT \"ItemCode\" FROM " + uti.schemaHana + "OITM " +
            "WHERE \"ItemCode\" is not null " + filtros + " ORDER BY \"ItemName\"";
            try
            {
                HanaDataReader hdr = db.HanaExecuteReaderNoSp(query);
                while (hdr.Read())
                {
                    OITM_E pd = BuscarArticulo(hdr.GetString(0));
                    lista.Add(pd);
                }
                hdr.Close();
            }
            catch { }
            return lista;
        }
        public OITM_E BuscarArticulo(string ItemCode)
        {
            OITM_E o = null;
            string query = "SELECT \"ItemCode\", \"ItemName\", \"FirmCode\" FROM " + uti.schemaHana + "OITM " +
                 "WHERE \"ItemCode\" = '" + ItemCode + "'";
            try
            {
                HanaDataReader hdr = db.HanaExecuteReaderNoSp(query);
                hdr.Read();
                o = new OITM_E();
                if (!hdr.IsDBNull(0)) { o.ItemCode = hdr.GetString(0); }
                if (!hdr.IsDBNull(1)) { o.ItemName = hdr.GetString(1); }                
                if (!hdr.IsDBNull(2)) { o.FirmCode = hdr.GetInt32(2); }                
                hdr.Close();
            }
            catch (Exception e) { throw new Exception(e.Message); }
            return o;
        }
        public string DatalistArticulos(OITM_E fil)
        {
            string lista = "";
            try
            {
                foreach (OITM_E o in ListarArticulos(fil))
                {
                    lista += "<option value=\"" + o.ItemCode + "\""
                            + " data-itemName=\"" + o.ItemName + "\""
                            + " data-firmCode=\"" + o.FirmCode + "\""
                            + " data-search=\"" + o.ItemCode + " " + o.ItemName + "\">"
                            + o.ItemCode + " - " + o.ItemName + "</option>";
                }
            }
            catch { }
            return lista;
        }
    }
}
