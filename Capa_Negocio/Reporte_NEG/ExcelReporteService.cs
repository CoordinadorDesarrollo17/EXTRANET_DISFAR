using Capa_Entidad.Inventario_ENT.TablasSql;
using ClosedXML.Excel; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Opcional, pero útil
using System.Text;
using System.Threading.Tasks;

namespace Capa_Negocio.Reporte_NEG
{
    public class ExcelReporteService
    {
        // Método exclusivo para Protocolos
        public byte[] GenerarReporteProtocolo(List<Protocolos_E> lista, DateTime inicio, DateTime fin)
        {
            // Creamos el libro de Excel
            using (var workbook = new XLWorkbook())
            {
                // Agregamos la hoja
                var ws = workbook.Worksheets.Add("Protocolos");

                // ----------------------------------------------------
                // 1. CONFIGURAR CABECERA DEL REPORTE (TÍTULO)
                // ----------------------------------------------------
                // Un título general combinando celdas
                ws.Range("A1:F1").Merge();
                ws.Cell("A1").Value = $"REPORTE DE PROTOCOLOS ({inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy})";
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // ----------------------------------------------------
                // 2. ENCABEZADOS DE LAS COLUMNAS (Fila 3)
                // ----------------------------------------------------
                int filaCabecera = 3;

                ws.Cell(filaCabecera, 1).Value = "Código Item";
                ws.Cell(filaCabecera, 2).Value = "Descripción";
                ws.Cell(filaCabecera, 3).Value = "Lote";
                ws.Cell(filaCabecera, 4).Value = "Usuario Carga";
                ws.Cell(filaCabecera, 5).Value = "Fecha Carga";
                ws.Cell(filaCabecera, 6).Value = "Tiene Archivo"; // Opcional

                // Estilo para la cabecera
                var rangoCabecera = ws.Range(filaCabecera, 1, filaCabecera, 6);
                rangoCabecera.Style.Font.Bold = true;
                rangoCabecera.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                rangoCabecera.Style.Font.FontColor = XLColor.White;

                // ----------------------------------------------------
                // 3. LLENADO DE DATOS
                // ----------------------------------------------------
                int fila = 4; // Empezamos a llenar desde la fila 4

                if (lista != null && lista.Count > 0)
                {
                    foreach (var item in lista)
                    {
                        ws.Cell(fila, 1).Value = item.ItemCode;
                        ws.Cell(fila, 2).Value = item.ItemName;
                        ws.Cell(fila, 3).Value = item.DistNumber; // Lote
                        ws.Cell(fila, 4).Value = item.OpCarga;    // Usuario
                        ws.Cell(fila, 5).Value = item.TiempoCarga; // Fecha

                        // Ejemplo lógico: Si RutaArchivo tiene algo, ponemos SI, sino NO
                        ws.Cell(fila, 6).Value = string.IsNullOrEmpty(item.RutaArchivo) ? "NO" : "SI";

                        fila++;
                    }
                }
                else
                {
                    ws.Cell(fila, 1).Value = "No se encontraron registros en este rango de fechas.";
                    ws.Range(fila, 1, fila, 6).Merge();
                }

                // ----------------------------------------------------
                // 4. AJUSTE FINAL
                // ----------------------------------------------------
                ws.Columns().AdjustToContents(); // Autoajustar ancho de columnas

                // ----------------------------------------------------
                // 5. GUARDAR EN MEMORIA
                // ----------------------------------------------------
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerarReporteRegSanitario(List<RegistrosSanitarios_E> datos, DateTime fechaInicio, DateTime fechaFin)
        {
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Reporte RegSanitario");

                // 1. TÍTULO
                ws.Cell(1, 1).Value = "REPORTE DE REGISTROS SANITARIOS";
                ws.Range("A1:D1").Merge();
                ws.Cell(1, 1).Style.Font.SetBold();
                ws.Cell(1, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                ws.Cell(2, 1).Value = $"Desde: {fechaInicio:dd/MM/yyyy} - Hasta: {fechaFin:dd/MM/yyyy}";
                ws.Range("A2:D2").Merge();
                ws.Cell(2, 1).Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                // 2. ENCABEZADOS (Fila 4)
                int filaHeader = 4;
                ws.Cell(filaHeader, 1).Value = "Serial (MnfSerial)";
                ws.Cell(filaHeader, 2).Value = "Operador";
                ws.Cell(filaHeader, 3).Value = "Fecha Carga";
                ws.Cell(filaHeader, 4).Value = "Archivo"; // Título de la columna

                // Estilo Encabezados
                var rangoHeader = ws.Range(filaHeader, 1, filaHeader, 4);
                rangoHeader.Style.Font.SetBold();
                rangoHeader.Style.Fill.SetBackgroundColor(XLColor.LightGray);
                rangoHeader.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                // 3. DATOS
                int fila = 5;
                if (datos != null && datos.Count > 0)
                {
                    foreach (var item in datos)
                    {
                        ws.Cell(fila, 1).Value = item.MnfSerial;
                        ws.Cell(fila, 2).Value = item.OpCarga;

                        // Fecha y Formato
                        ws.Cell(fila, 3).Value = item.TiempoCarga;
                        ws.Cell(fila, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";


                        // Ahora validamos si está vacío o no:
                        ws.Cell(fila, 4).Value = string.IsNullOrEmpty(item.RutaArchivo) ? "NO" : "SI";
                        // ---------------------------------------------------------

                        fila++;
                    }
                }
                else
                {
                    ws.Cell(fila, 1).Value = "No hay datos para mostrar.";
                    ws.Range(fila, 1, fila, 4).Merge();
                }

                // 4. FORMATO FINAL
                ws.Columns().AdjustToContents();

                // 5. RETORNAR BYTES
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public byte[] GenerarReporteEspTecnica(List<EspecificacionesTecnicas_E> lista, DateTime inicio, DateTime fin)
        {
            using (var workbook = new XLWorkbook())
            {
                // Nombre de la pestaña (Excel no permite nombres muy largos)
                var ws = workbook.Worksheets.Add("Esp. Tecnicas");

                // ----------------------------------------------------
                // 1. CONFIGURAR CABECERA DEL REPORTE (TÍTULO)
                // ----------------------------------------------------
                // Fusionamos 6 columnas (A hasta F)
                ws.Range("A1:F1").Merge();
                ws.Cell("A1").Value = $"REPORTE DE ESPECIFICACIONES TÉCNICAS ({inicio:dd/MM/yyyy} - {fin:dd/MM/yyyy})";

                // Estilo del Título Principal
                ws.Cell("A1").Style.Font.Bold = true;
                ws.Cell("A1").Style.Font.FontSize = 14; // Un poco más grande
                ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell("A1").Style.Fill.BackgroundColor = XLColor.LightGray;

                // ----------------------------------------------------
                // 2. ENCABEZADOS DE LAS COLUMNAS (Fila 3)
                // ----------------------------------------------------
                int filaCabecera = 3;

                ws.Cell(filaCabecera, 1).Value = "Código Item";
                ws.Cell(filaCabecera, 2).Value = "Descripción Producto";
                ws.Cell(filaCabecera, 3).Value = "Nombre Ref. / Archivo"; // Campo específico de Esp. Tecnica
                ws.Cell(filaCabecera, 4).Value = "Usuario Carga";
                ws.Cell(filaCabecera, 5).Value = "Fecha Carga";
                ws.Cell(filaCabecera, 6).Value = "Tiene Archivo";

                // Estilo para la cabecera (Fondo Verde para diferenciar de Protocolos)
                var rangoCabecera = ws.Range(filaCabecera, 1, filaCabecera, 6);
                rangoCabecera.Style.Font.Bold = true;
                rangoCabecera.Style.Fill.BackgroundColor = XLColor.SeaGreen; // Color distintivo
                rangoCabecera.Style.Font.FontColor = XLColor.White;
                rangoCabecera.Style.Border.BottomBorder = XLBorderStyleValues.Medium;

                // ----------------------------------------------------
                // 3. LLENADO DE DATOS
                // ----------------------------------------------------
                int fila = 4;

                if (lista != null && lista.Count > 0)
                {
                    foreach (var item in lista)
                    {
                        ws.Cell(fila, 1).Value = item.ItemCode;
                        ws.Cell(fila, 2).Value = item.ItemName;
                        ws.Cell(fila, 3).Value = item.Nombre;   // Dato específico
                        ws.Cell(fila, 4).Value = item.OpCarga;

                        // Fecha con formato
                        ws.Cell(fila, 5).Value = item.TiempoCarga;
                        ws.Cell(fila, 5).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";

                        // Lógica de archivo (SI/NO)
                        // Nota: Usamos RutaArchivo porque es donde se armó la ruta completa en la Capa de Datos
                        ws.Cell(fila, 6).Value = string.IsNullOrEmpty(item.RutaArchivo) ? "NO" : "SI";

                        // Centrar la columna de SI/NO para que se vea ordenado
                        ws.Cell(fila, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        fila++;
                    }
                }
                else
                {
                    ws.Cell(fila, 1).Value = "No se encontraron registros en este rango de fechas.";
                    ws.Range(fila, 1, fila, 6).Merge();
                    ws.Cell(fila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // ----------------------------------------------------
                // 4. AJUSTE FINAL
                // ----------------------------------------------------
                ws.Columns().AdjustToContents();

                // ----------------------------------------------------
                // 5. GUARDAR EN MEMORIA
                // ----------------------------------------------------
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }
    }
}