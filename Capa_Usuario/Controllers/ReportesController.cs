using Capa_Datos.Inventario_DAO;
using Capa_Datos.Inventario_DAO.TablasSql; // Aquí viven tus clases _D
using Capa_Entidad.Inventario_ENT.TablasSql;
using Capa_Negocio.Reporte_NEG;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO; // Necesario para Path.Combine si se usa aquí
using System.Text;
using System.Web.Mvc;

namespace Capa_Usuario.Controllers
{
    public class ReportesController : Controller
    {
        // ---------------------------------------------------
        // 1. INSTANCIAS DE SERVICIOS Y DATOS
        // ---------------------------------------------------
        private readonly ExcelReporteService _excelService = new ExcelReporteService();

        // Datos para Protocolos
        private readonly Protocolos_D _protocolosDatos = new Protocolos_D();

        //Datos para Registros Sanitarios 
        private readonly RegistrosSanitarios_D _regSanitariosDatos = new RegistrosSanitarios_D();

        private readonly EspecificacionesTecnicas_D _espTecnicasDatos = new EspecificacionesTecnicas_D();

        // ---------------------------------------------------
        // 2. MÉTODO AUXILIAR: Validación de Fechas
        // ---------------------------------------------------
        private string ValidarReglasDeNegocio(DateTime inicio, DateTime fin)
        {
            if (fin < inicio)
            {
                return "La fecha final no puede ser menor a la fecha de inicio.";
            }

            TimeSpan diferencia = fin - inicio;
            int diasSolicitados = diferencia.Days + 1;
            int diasEnElMes = DateTime.DaysInMonth(inicio.Year, inicio.Month);

            // Regla: No permitir rangos excesivos (opcional, ajusta si quieres permitir más)
            if (diasSolicitados > 35) // Dejamos un margen de 35 días por si acaso
            {
                return $"El rango seleccionado ({diasSolicitados} días) es demasiado amplio. Por favor filtre por mes.";
            }

            return string.Empty;
        }

        // ---------------------------------------------------
        // 3. REPORTE MODULO: PROTOCOLOS
        // ---------------------------------------------------
        [HttpPost]
        public ActionResult DescargarProtocolos(string txtOpCarga, DateTime txtFechaInicio, DateTime txtFechaFin)
        {
            try
            {
                // Validación
                string error = ValidarReglasDeNegocio(txtFechaInicio, txtFechaFin);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Mensaje"] = error;
                    return RedirectToAction("GestionProtocolo", "Documentos");
                }

                // Obtener Datos
                List<Protocolos_E> datos = _protocolosDatos.ListarReporteProtocolos(txtOpCarga, txtFechaInicio, txtFechaFin);

                if (datos == null || datos.Count == 0)
                {
                    TempData["Mensaje"] = "No se encontraron registros con esos filtros.";
                    return RedirectToAction("GestionProtocolo", "Documentos");
                }

                // Generar Excel
                byte[] archivo = _excelService.GenerarReporteProtocolo(datos, txtFechaInicio, txtFechaFin);

                string nombreArchivo = $"Protocolos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error generando el reporte: {ex.Message}";
                return RedirectToAction("GestionProtocolo", "Documentos");
            }
        }

        // ---------------------------------------------------
        // 4. REPORTE MODULO: REGISTROS SANITARIOS
        // ---------------------------------------------------
        [HttpPost]
        public ActionResult DescargarReporteRegSanitario(string opCarga, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // 1. Validaciones
                string error = ValidarReglasDeNegocio(fechaInicio, fechaFin);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Mensaje"] = error;
                    return RedirectToAction("GestionRegistroSanitario", "Documentos");
                }

                // 2. Obtener la lista de BD
                var lista = _regSanitariosDatos.ListarReporteRegSanitario(opCarga, fechaInicio, fechaFin);

                if (lista == null || lista.Count == 0)
                {
                    TempData["Mensaje"] = "No se encontraron Registros Sanitarios en ese rango de fechas.";
                    return RedirectToAction("GestionRegistroSanitario", "Documentos");
                }

                // 3. Generar Excel
                byte[] archivo = _excelService.GenerarReporteRegSanitario(lista, fechaInicio, fechaFin);

                // 4. Retornar archivo .xlsx
                string nombreArchivo = $"RegSanitario_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al generar el reporte: {ex.Message}";
                return RedirectToAction("GestionRegistroSanitario", "Documentos");
            }
        }

        // ---------------------------------------------------
        // 5. NUEVO REPORTE: ESPECIFICACIONES TÉCNICAS
        // ---------------------------------------------------
        [HttpPost]
        public ActionResult DescargarReporteEspTecnica(string opCarga, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // 1. Validaciones
                string error = ValidarReglasDeNegocio(fechaInicio, fechaFin);
                if (!string.IsNullOrEmpty(error))
                {
                    TempData["Mensaje"] = error;
                    return RedirectToAction("GestionEspecificacionTecnica", "Documentos");
                }

                // 2. Obtener Datos
                var lista = _espTecnicasDatos.ListarReporteEspTecnica(opCarga, fechaInicio, fechaFin);

                if (lista == null || lista.Count == 0)
                {
                    TempData["Mensaje"] = "No se encontraron Especificaciones Técnicas en ese rango de fechas.";
                    return RedirectToAction("GestionEspecificacionTecnica", "Documentos");
                }

                // 3. Generar Excel
                byte[] archivo = _excelService.GenerarReporteEspTecnica(lista, fechaInicio, fechaFin);

                // 4. Retornar archivo
                string nombreArchivo = $"EspTecnicas_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", nombreArchivo);
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al generar el reporte: {ex.Message}";
                return RedirectToAction("GestionEspecificacionTecnica", "Documentos");
            }
        }
    }
}