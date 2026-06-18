using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Capa_Negocio.Inventario_NEG.TablasSql
{
    public class Protocolos_N
    {
        Protocolos_D _datos = new Protocolos_D();
        public int AgregarProtocolo(Protocolos_E o)
        {
            if (o.ArchivoPdf != null)
            {
                if (o.ArchivoPdf.ContentType != "application/pdf") { throw new Exception("Debe elegir un archivo pdf"); }
                if (o.ArchivoPdf.ContentLength > 10485760) { throw new Exception("No puedes cargar un archivo superior a 10Mb"); }
            }
            if (o.ItemCode == null) { throw new Exception("Debe llenar un Codigo de Articulo"); }
            //o.ItemCode = o.ItemCode.Replace(" ", "");
            if (o.ItemCode == "") { throw new Exception("Debe llenar un Codigo de Articulo"); }
            if (o.ItemName == null || o.ItemName == "") { throw new Exception("Debe llenar un Codigo de Articulo y descripcion"); }
            if (o.DistNumber == null || o.DistNumber == "") { throw new Exception("Debe elegir un lote"); }
            if (o.MnfSerial == null || o.MnfSerial == "") { throw new Exception("Debe ingresar un registro valido"); }
            if (string.IsNullOrWhiteSpace(o.ExpDate)) { throw new Exception("Debe llenar fecha de vencimiento"); }
            return _datos.AgregarProtocolo(o);
        }
        public Protocolos_E BuscarProtocolo(string ItemCode, string DistNumber)
        {
            return _datos.BuscarProtocolo(ItemCode, DistNumber);
        }

        public (Helper_E, List<Protocolos_E>) ListarProtocolos(Protocolos_E filtro = null)
        {
            var parametros = new Dictionary<string, object>();
            StringBuilder condiciones = new StringBuilder();
            var helper = new Helpers();

            if (filtro != null)
            {
                // CORRECCIÓN AQUÍ: Agregamos "x." antes de ItemCode
                if (!string.IsNullOrWhiteSpace(filtro.ItemCode))
                {
                    condiciones.AppendLine("AND x.ItemCode LIKE @ItemCode");
                    // CUIDADO: Asegúrate que SanitizarTexto no borre guiones (-) o puntos si tus códigos los tienen
                    parametros["@ItemCode"] = $@"%{filtro.ItemCode.Trim()}%";
                }

                // CORRECCIÓN AQUÍ: pr.ItemName (porque viene del JOIN) o x.ItemName si estuviera en la subconsulta
                // Viendo tu Query, ItemName viene de 'pr', así que usa pr.ItemName
                if (!string.IsNullOrWhiteSpace(filtro.ItemName))
                {
                    condiciones.AppendLine("AND pr.ItemName LIKE @ItemName");
                    parametros["@ItemName"] = $@"%{filtro.ItemName.Trim()}%";
                }

                // CORRECCIÓN AQUÍ: Agregamos "x."
                if (!string.IsNullOrWhiteSpace(filtro.DistNumber))
                {
                    condiciones.AppendLine("AND x.DistNumber LIKE @DistNumber");
                    parametros["@DistNumber"] = $@"%{filtro.DistNumber.Trim()}%";
                }

                // CORRECCIÓN AQUÍ: Agregamos "x."
                if (!string.IsNullOrWhiteSpace(filtro.OpCarga))
                {
                    condiciones.AppendLine("AND x.OpCarga LIKE @OpCarga");
                    parametros["@OpCarga"] = $@"%{filtro.OpCarga.Trim()}%";
                }

                // CORRECCIÓN AQUÍ: Agregamos "x."
                if (filtro.TiempoCarga > DateTime.MinValue)
                {
                    condiciones.AppendLine("AND CONVERT(date, x.TiempoCarga) = @TiempoCarga");
                    parametros["@TiempoCarga"] = filtro.TiempoCarga.Date;
                }
            }

            return _datos.ListarProtocolos(condiciones.ToString(), parametros);
        }
        public void EliminarProtocolo(string itemCode, string distNumber)
        {
            new Protocolos_D().EliminarProtocolo(itemCode, distNumber);
        }

        public bool VerificarLoteExistente(string itemCode, string lote)
        {
            var d = new Protocolos_D();
            return d.ExisteLote(itemCode, lote);
        }
    }
}
