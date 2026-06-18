using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Capa_Negocio.Inventario_NEG.TablasMysql
{
    public class EspecificacionesTecnicas_N
    {
        EspecificacionesTecnicas_D _datos = new EspecificacionesTecnicas_D();
        public int AgregarEspTecnica(EspecificacionesTecnicas_E o)
        {
            if (o.ArchivoPdf == null) { throw new Exception("Debe elegir un archivo"); }
            if (o.ArchivoPdf.ContentType != "application/pdf") { throw new Exception("Debe elegir un archivo pdf"); }
            if (o.ArchivoPdf.ContentLength > 10485760) { throw new Exception("No puedes cargar un archivo superior a 10Mb"); }
            if (o.ItemCode == null) { throw new Exception("Debe llenar un Codigo de Articulo"); }
            if (o.ItemCode == "") { throw new Exception("Debe llenar un Codigo de Articulo"); }
            if (o.ItemName == null || o.ItemName == "") { throw new Exception("Debe llenar un Codigo de Articulo y descripcion"); }
            o.Nombre = "ET";
            return _datos.AgregarEspTecnica(o);
        }
        public EspecificacionesTecnicas_E BuscarEspTecnica(string ItemCode)
        {
            return _datos.BuscarEspTecnica(ItemCode);
        }

        public (Helper_E, List<EspecificacionesTecnicas_E>) ListarEspTecnicas(EspecificacionesTecnicas_E filtro = null)
        {
            var condiciones = new StringBuilder();
            var parametros = new Dictionary<string, object>();
            var helper = new Helpers(); // Asumo que necesitas instanciar esto si lo usas para sanitizar

            if (filtro != null)
            {
                // CORRECCIÓN 1: Agregar "x." para desambiguar
                if (!string.IsNullOrWhiteSpace(filtro.ItemCode))
                {
                    condiciones.AppendLine("AND x.ItemCode LIKE @ItemCode");
                    parametros["@ItemCode"] = $@"%{filtro.ItemCode.Trim()}%";
                }

                // CORRECCIÓN 2: ItemName viene de la tabla Productos (pr)
                if (!string.IsNullOrWhiteSpace(filtro.ItemName))
                {
                    condiciones.AppendLine("AND pr.ItemName LIKE @ItemName");
                    parametros["@ItemName"] = $@"%{filtro.ItemName.Trim()}%";
                }

                // CORRECCIÓN 3: Agregar "x." por seguridad
                if (!string.IsNullOrWhiteSpace(filtro.Nombre))
                {
                    condiciones.AppendLine("AND x.Nombre LIKE @Nombre");
                    parametros["@Nombre"] = $@"%{filtro.Nombre.Trim()}%";
                }

                // CORRECCIÓN 4: Agregar "x."
                if (!string.IsNullOrWhiteSpace(filtro.OpCarga))
                {
                    condiciones.AppendLine("AND x.OpCarga LIKE @OpCarga");
                    parametros["@OpCarga"] = $@"%{filtro.OpCarga.Trim()}%";
                }

                // CORRECCIÓN 5: Agregar "x."
                if (filtro.TiempoCarga != DateTime.MinValue)
                {
                    condiciones.AppendLine("AND CONVERT(date, x.TiempoCarga) = @TiempoCarga");
                    parametros["@TiempoCarga"] = filtro.TiempoCarga.Date;
                }
            }

            return _datos.ListarEspTecnicas(condiciones.ToString(), parametros);
        }

        public Helper_E EliminarEspTecnicas(string ItemCode)
        {
            return new EspecificacionesTecnicas_D().EliminarEspTecnicas(ItemCode);
        }

        public bool VerificarEspTecExistente(string itemCode)
        {
            var d = new EspecificacionesTecnicas_D();
            return d.ExisteEspTec(itemCode);
        }
    }
}
