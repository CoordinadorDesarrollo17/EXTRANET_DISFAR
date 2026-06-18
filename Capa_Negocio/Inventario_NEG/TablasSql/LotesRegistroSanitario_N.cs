using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Text;
namespace Capa_Negocio.Inventario_NEG.TablasSql
{
    public class LotesRegistroSanitario_N
    {
        private readonly LotesRegistroSanitario_D _datos = new LotesRegistroSanitario_D();
        private readonly Helpers _helper = new Helpers();

        public (Helper_E, List<LotesRegistroSanitario_E>) ListarLotesRegistroSanitario(LotesRegistroSanitario_E filtros = null)
        {
            var parametros = new Dictionary<string, object>();
            StringBuilder condiciones = new StringBuilder();
            var helper = new Helpers();

            if (filtros != null)
            {
                if (!string.IsNullOrWhiteSpace(filtros.ItemCode))
                {
                    condiciones.AppendLine("AND Pr.ItemCode LIKE @ItemCode");
                    parametros["@ItemCode"] = $@"%{filtros.ItemCode.Trim()}%";
                }
                if (!string.IsNullOrWhiteSpace(filtros.ItemName))
                {
                    condiciones.AppendLine("AND Pr.ItemName LIKE @ItemName");
                    parametros["@ItemName"] = $@"%{filtros.ItemName.Trim()}%";
                }
                if (!string.IsNullOrWhiteSpace(filtros.ExpDate) && DateTime.TryParse(filtros.ExpDate, out DateTime fecha))
                {
                    condiciones.AppendLine("AND CONVERT(date, Lt.ExpDate) = @ExpDate");
                    parametros["@ExpDate"] = fecha.Date;
                }
                if (!string.IsNullOrWhiteSpace(filtros.DistNumber))
                {
                    condiciones.AppendLine("AND Lt.DistNumber LIKE @DistNumber");
                    parametros["@DistNumber"] = $@"%{filtros.DistNumber.Trim()}%";
                }
                if (!string.IsNullOrWhiteSpace(filtros.MnfSerial))
                {
                    condiciones.AppendLine("AND Lt.MnfSerial LIKE @MnfSerial");
                    parametros["@MnfSerial"] = $@"%{filtros.MnfSerial.Trim()}%";
                }
            }

            return _datos.ListarLotesRegistroSanitario(condiciones.ToString(), parametros);
        }

    }
}
