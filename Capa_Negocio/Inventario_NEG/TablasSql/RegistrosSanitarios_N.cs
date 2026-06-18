using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad;
using Capa_Entidad.Inventario_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Capa_Negocio.Inventario_NEG.TablasSql
{
    public class RegistrosSanitarios_N
    {
        RegistrosSanitarios_D _datos = new RegistrosSanitarios_D();
        public int AgregarRegSanitario(RegistrosSanitarios_E o)
        {
            if (o.ArchivoPdf == null)
                throw new Exception("Debe elegir un archivo");
            if (o.ArchivoPdf.ContentType != "application/pdf")
                throw new Exception("Debe elegir un archivo pdf");
            if (o.ArchivoPdf.ContentLength > 10485760)
                throw new Exception("No puedes cargar un archivo superior a 10Mb");
            if (string.IsNullOrEmpty(o.MnfSerial))
                throw new Exception("El número de registro sanitario no puede ser vacío.");
            var registroSanitarioExistente = new TablasHana.OBTN_N().ObtenerRegistrosSanitariosExistentes(new Capa_Entidad.Inventario_ENT.TablasHana.OBTN_E { MnfSerial = o.MnfSerial });
            if (registroSanitarioExistente == null || registroSanitarioExistente.Count <= 0)
                throw new Exception("El número de registro sanitario ingresado no existe.");
            if (string.IsNullOrEmpty(o.MnfSerial) && o.MnfSerial.Contains("/"))
                throw new Exception("Debe llenar un Nro de Registro Sanitario sin / ");
            o.MnfSerial = o.MnfSerial.Replace("/", " ");
            return _datos.AgregarRegSanitario(o);
        }
        public RegistrosSanitarios_E BuscarRegSanitario(string MnfSerial)
        {
            return _datos.BuscarRegSanitario(MnfSerial);
        }

        public (Helper_E, List<RegistrosSanitarios_E>) ListarRegSanitarios(RegistrosSanitarios_E filtro = null)
        {
            var condiciones = new StringBuilder();
            var parametros = new Dictionary<string, object>();
           

            if (filtro != null)
            {
                if (!string.IsNullOrWhiteSpace(filtro.MnfSerial))
                {
                    condiciones.AppendLine("AND x.MnfSerial LIKE @MnfSerial");
                    parametros["@MnfSerial"] = $@"%{filtro.MnfSerial.Trim()}%";
                }

                // ✅ NUEVO FILTRO: ItemName (viene de tabla Productos via JOIN)
                if (!string.IsNullOrWhiteSpace(filtro.ItemName))
                {
                    condiciones.AppendLine("AND pr.ItemName LIKE @ItemName");
                    parametros["@ItemName"] = $@"%{filtro.ItemName.Trim()}%";
                }

                if (!string.IsNullOrWhiteSpace(filtro.OpCarga))
                {
                    condiciones.AppendLine("AND x.OpCarga LIKE @OpCarga");
                    parametros["@OpCarga"] = $@"%{filtro.OpCarga.Trim()}%";
                }

                if (filtro.TiempoCarga > DateTime.MinValue)
                {
                    // 1. Definimos el rango: Desde las 00:00 de hoy hasta las 00:00 de MAÑANA
                    condiciones.AppendLine("AND x.TiempoCarga >= @FechaInicio AND x.TiempoCarga < @FechaFin");

                    // 2. Pasamos los parámetros
                    // @FechaInicio = 25/09/2025 00:00:00
                    parametros["@FechaInicio"] = filtro.TiempoCarga.Date;

                    // @FechaFin = 26/09/2025 00:00:00 (El límite superior, sin tocarlo)
                    parametros["@FechaFin"] = filtro.TiempoCarga.Date.AddDays(1);
                }
            }

            return _datos.ListarRegSanitarios(condiciones.ToString(), parametros);
        }
        public Helper_E EliminarRegSanitario(string MnfSerial)
        {
            return new RegistrosSanitarios_D().EliminarRegSanitario(MnfSerial);
        }

        public bool VerificarSerialExistente(string mnfserial)
        {
            var d = new RegistrosSanitarios_D();
            return d.ExisteSerial(mnfserial);
        }
    }
}
