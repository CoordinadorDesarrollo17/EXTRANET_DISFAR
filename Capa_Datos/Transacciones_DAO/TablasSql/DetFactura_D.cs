using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
namespace Capa_Datos.Transacciones_DAO.TablasSql
{
    public class DetFactura_D
    {
        DBHelper db = new DBHelper(); 
        RegistrosSanitarios_D oersD = new RegistrosSanitarios_D();
        EspecificacionesTecnicas_D oeetD = new EspecificacionesTecnicas_D();
        Protocolos_D oeptD = new Protocolos_D();
        public List<DetFactura_E> ListaDetalleFactura(int baseEntry)
        {
            var lista = new List<DetFactura_E>();
            string query = "SELECT ItemCode,BatchNum,ItemName,LineNum from DetFactura WHERE BaseEntry=@BaseEntry";
            try
            {
                SqlDataReader dr = db.SqlExecuteReaderNoSp(query, new List<string>() { "@BaseEntry" }, baseEntry);
                while (dr.Read())
                {
                    var o = new DetFactura_E();
                    if (!dr.IsDBNull(0)) o.ItemCode = dr.GetString(0); 
                    if (!dr.IsDBNull(1)) o.BatchNum = dr.GetString(1); 
                    if (!dr.IsDBNull(2)) o.ItemName = dr.GetString(2); 
                    if (!dr.IsDBNull(3)) o.LineNum = dr.GetInt32(3); 
                    var loteRegistroSanitario = new LotesRegistroSanitario_D().BuscarLotesRegistroSanitario(o.ItemCode, o.BatchNum);
                    if (loteRegistroSanitario != null)
                    {
                        //o.RegistroSanitario = uti.directorioFileServer + $@"Registros/{loteRegistroSanitario.MnfSerial}";
                        o.RegistroSanitario = oersD.BuscarRegSanitario(loteRegistroSanitario.MnfSerial);
                    }
                    o.ET = oeetD.BuscarEspTecnica(o.ItemCode);
                    o.Protocolo = oeptD.BuscarProtocolo(o.ItemCode, o.BatchNum);
                    lista.Add(o);
                }
                dr.Close();
            }
            catch (Exception e) { throw new Exception("Error en conexion: " + e.Message); }
            return lista;
        }
    }
}
