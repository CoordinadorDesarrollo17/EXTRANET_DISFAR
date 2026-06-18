using Capa_Datos.Transacciones_DAO.TablasSql;
using Capa_Entidad;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Capa_Negocio.Transacciones_NEG.TablasSql
{
    public class Factura_N
    {
        private readonly Factura_D _datos = new Factura_D();
        private readonly Helpers _helper = new Helpers();
        public Factura_E BuscarComprobante(int DocEntry, Usuario_E user)
        {
            return _datos.BuscarComprobante(DocEntry, user);
        }
        public (Helper_E, List<Factura_E>) ListarComprobantes(Factura_E filtro = null)
        {
            var parametros = new Dictionary<string, object>();
            StringBuilder condiciones = new StringBuilder();

            if (filtro != null)
            {
                if (filtro.DocEntry > 0)
                {
                    condiciones.AppendLine("AND FT.DocEntry = @DocEntry");
                    parametros["@DocEntry"] = filtro.DocEntry;
                }

                if (!string.IsNullOrWhiteSpace(filtro.CardCode))
                {
                    condiciones.AppendLine("AND FT.CardCode LIKE @CardCode");
                    parametros["@CardCode"] = $"%{filtro.CardCode}%";
                }
                if (!string.IsNullOrWhiteSpace(filtro.CardName))
                {
                    condiciones.AppendLine("AND FT.CardName LIKE @CardName");
                    parametros["@CardName"] = $"%{filtro.CardName}%";
                }
                if (!string.IsNullOrWhiteSpace(filtro.Correlativo))
                {
                    condiciones.AppendLine("AND FT.Correlativo LIKE CONCAT('%', @Correlativo, '%')");
                    parametros["@Correlativo"] = $"%{filtro.Correlativo.Trim()}%";
                }

                if (!string.IsNullOrWhiteSpace(filtro.Correlativo2))
                {
                    condiciones.AppendLine("AND FT.Correlativo LIKE CONCAT('%', @Correlativo2, '%')");
                    parametros["@Correlativo2"] = $"%{filtro.Correlativo2.Trim()}%";
                }
                if (!string.IsNullOrWhiteSpace(filtro.DocDate))
                {
                    condiciones.AppendLine("AND CONVERT(date, FT.DocDate) = @DocDate");
                    if (DateTime.TryParse(filtro.DocDate, out var fecha))
                    {
                        parametros["@DocDate"] = fecha.Date;
                    }
                }
                if (!string.IsNullOrWhiteSpace(filtro.FechaInicio) &&
                    DateTime.TryParse(filtro.FechaInicio, out DateTime fechaInicio))
                {
                    condiciones.AppendLine("AND DocDate >= @FechaInicio");
                    parametros["@FechaInicio"] = fechaInicio.Date;
                }
                if (!string.IsNullOrWhiteSpace(filtro.FechaFin) &&
                    DateTime.TryParse(filtro.FechaFin, out DateTime fechaFin))
                {
                    condiciones.AppendLine("AND DocDate <= @FechaFin");
                    parametros["@FechaFin"] = fechaFin.Date;
                }
            }
            return _datos.ListarComprobantes(condiciones.ToString(), parametros);
        }

        public Helper_E ValidarListarComprobantes(Factura_E filtro)
        {
            var resultado = new Helper_E();

            if (filtro.CardCode == null) { return _helper.CrearRespuestaError("Debe seleccionar un cliente"); }
            if (filtro.FechaInicio == null) { return _helper.CrearRespuestaError("Debe elegir fecha inicial"); }
            if (filtro.FechaFin == null) { return _helper.CrearRespuestaError("Debe elegir fecha final"); }

            DateTime DtFecIni = DateTime.Parse(filtro.FechaInicio);
            DateTime DtFecFin = DateTime.Parse(filtro.FechaFin);

            if (DtFecIni.CompareTo(DtFecFin) > 0) { return _helper.CrearRespuestaError("La fecha inicial no puede ser mayor a la fecha final"); }
            if ((DtFecFin - DtFecIni).Days > 186) { return _helper.CrearRespuestaError("El rango de fechas no puede superar los 6 meses"); }

            return resultado;
        }
    }
}
