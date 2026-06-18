using Capa_Datos;
using Capa_Datos.Inventario_DAO.TablasSql;
using Capa_Entidad.Inventario_ENT.TablasHana;
using Capa_Entidad.Inventario_ENT.TablasSql;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
using Capa_Negocio;
using Capa_Negocio.Inventario_NEG.TablasHana;
using Capa_Negocio.Inventario_NEG.TablasMysql;
using Capa_Negocio.Inventario_NEG.TablasSql;
using Capa_Negocio.SociosNegocios_NEG.TablasSql;
using Capa_Negocio.Transacciones_NEG.TablasSql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Capa_Usuario.Controllers
{
    public class DocumentosController : Controller
    {
        private readonly Factura_N facturaNegocio = new Factura_N();
        private readonly Clientes_N clientesNegocio = new Clientes_N();

        public ActionResult GestionDocumentos(LotesRegistroSanitario_E filtros, int idOperacion = 9)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                ViewBag.fil = filtros;
                var lista = new LotesRegistroSanitario_N().ListarLotesRegistroSanitario(filtros).Item2;
                return View(lista);
            }

            else if (acceso == "E_Login") { return RedirectToAction("Index","Home"); }
            else { return RedirectToAction("ErrorOperacion"); }
        }

        public ActionResult GestionProtocolo(int idOperacion = 10)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var resultado = new Protocolos_D().ListarProtocolos();
                ViewBag.Mensaje = resultado.Item1;
                return View(resultado.Item2);
            }
            else if (acceso == "E_Login") { return RedirectToAction("Index","Home"); }
            else { return RedirectToAction("ErrorOperacion"); }
        }

        [HttpPost]
        public ActionResult GestionProtocolo(Protocolos_E obj, int idOperacion = 10)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                try
                {
                    Usuario_E user = Session["Usuario"] as Usuario_E;
                    obj.OpCarga = user?.Nombre ?? "Desconocido";

                    new Capa_Negocio.Inventario_NEG.TablasSql.Protocolos_N()
                        .AgregarProtocolo(obj);

                    TempData["Mensaje"] = "Protocolo guardado correctamente.";
                }
                catch (Exception e)
                {
                    TempData["Mensaje"] = e.Message;
                }

                return RedirectToAction("GestionProtocolo");
            }
            else if (acceso == "E_Login")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorOperacion");
            }
        }


        public ActionResult GestionRegistroSanitario(int idOperacion = 13)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso != "C_Access")
            {
                return acceso == "E_Login" ? RedirectToAction("Index", "Home") : RedirectToAction("ErrorOperacion");
            }

            return CargarVistaGestionRegistroSanitario();
        }

        [HttpPost]
        public ActionResult GestionRegistroSanitario(RegistrosSanitarios_E obj, int idOperacion = 13)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso != "C_Access")
            {
                return acceso == "E_Login" ? RedirectToAction("Index", "Home") : RedirectToAction("ErrorOperacion","Home");
            }

            try
            {
                var oersN = new Capa_Negocio.Inventario_NEG.TablasSql.RegistrosSanitarios_N();
                var user = Session["Usuario"] as Usuario_E;
                obj.OpCarga = user?.Nombre ?? "Desconocido";

                oersN.AgregarRegSanitario(obj);

                TempData["Mensaje"] = "Registro sanitario guardado correctamente.";
                return RedirectToAction("GestionRegistroSanitario");
            }
            catch (Exception ex)
            {
                return CargarVistaGestionRegistroSanitario(ex.Message);
            }
        }

        //Controllador Auxiliar para evitar sobrecarga
        private ActionResult CargarVistaGestionRegistroSanitario(string mensaje = null)
        {
            var oersN = new Capa_Negocio.Inventario_NEG.TablasSql.RegistrosSanitarios_N();
            var resultado = oersN.ListarRegSanitarios();

            var seriales = new Capa_Negocio.Inventario_NEG.TablasHana.OBTN_N().ObtenerRegistrosSanitariosExistentes(null);
            ViewBag.SerialesDisponibles = seriales;

            ViewBag.Mensaje = TempData["Mensaje"] ?? mensaje ?? resultado.Item1.Mensaje;
            return View("GestionRegistroSanitario", resultado.Item2);
        }

        public ActionResult GestionEspecificacionTecnica(int idOperacion = 14)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var resultado = new EspecificacionesTecnicas_D().ListarEspTecnicas();
                ViewBag.Mensaje = resultado.Item1;
                return View(resultado.Item2);
            }
            else if (acceso == "E_Login") { return RedirectToAction("Index", "Home"); }
            else { return RedirectToAction("ErrorOperacion", "Home"); }
        }

        [HttpPost]
        public ActionResult GestionEspecificacionTecnica(EspecificacionesTecnicas_E obj, int idOperacion = 14)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                try
                {
                    var oeetN = new Capa_Negocio.Inventario_NEG.TablasMysql.EspecificacionesTecnicas_N();
                    var user = Session["Usuario"] as Usuario_E;
                    obj.OpCarga = user?.Nombre ?? "Desconocido";

                    oeetN.AgregarEspTecnica(obj);

                    TempData["Mensaje"] = "Especificación técnica guardada correctamente.";
                }
                catch (Exception e)
                {
                    TempData["Mensaje"] = e.Message;
                }

                return RedirectToAction("GestionEspecificacionTecnica");
            }
            else if (acceso == "E_Login")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorOperacion", "Home");
            }
        }

        public ActionResult ConsultarComprobantes(Factura_E obj, int idOperacion = 3)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                var user = (Usuario_E)Session["Usuario"];
                ViewBag.Tipo = user.TipoUsuarioId;
                //if (user.TipoUsuarioId != 3) { ViewBag.Clientes = ousrN.ListarUsuarios(new Usuario_E { TipoUsuarioId = 3 }); }
                ViewBag.Clientes = new Clientes_N().ListarClientes(null).OrderBy(c => c.CardName).ToList();

                if (obj != null && string.IsNullOrWhiteSpace(obj.CardCode) && user.TipoUsuarioId == 3)
                    obj.CardCode = user.Usuario;
                var lista = new Factura_N().ListarComprobantes(obj).Item2;
                return View("ConsultarComprobantes", lista);

            }
            else if (acceso == "E_Login") { return RedirectToAction("Index", "Home"); }
            else { return RedirectToAction("ErrorOperacion", "Home"); }
        }

        public ActionResult DetalleComprobante(int DocEntry, int idOperacion = 6)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var user = (Usuario_E)Session["Usuario"];
                Factura_E comprobante = new Factura_N().BuscarComprobante(DocEntry, user);
                try
                {
                    ViewBag.NroComprobante = comprobante.Correlativo;
                    ViewBag.Fecha = comprobante.DocDate;
                    return PartialView("_DetalleComprobante", comprobante.Detalle);
                }
                catch
                {
                    return PartialView("_DetalleComprobante", new List<DetFactura_E>());
                }
            }
            else if (acceso == "E_Login") { return RedirectToAction("Index", "Home"); }
            else { return RedirectToAction("ErrorOperacion", "Home"); }
        }

        //metodos
        public FileResult DocumentoRegSanit(string id, int idOperacion = 15)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var user = (Usuario_E)Session["Usuario"];
                var registroSanitario = new RegistrosSanitarios_N().BuscarRegSanitario(id);
                if (registroSanitario == null || string.IsNullOrEmpty(registroSanitario.RutaArchivo))
                    return null;
                string contentType = MimeMapping.GetMimeMapping(registroSanitario.RutaArchivo); // Obtiene el tipo MIME
                return File(registroSanitario.RutaArchivo, contentType, Path.GetFileName(registroSanitario.RutaArchivo));
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }
        public FileResult DocumentoEspTecnica(string id, int idOperacion = 15)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var user = Session["Usuario"] as Usuario_E;
                bool caracterEspecial = id.Contains("ampersand");
                if (caracterEspecial)
                    id = id.Replace("ampersand", "&");
                var especificacionTecnica = new Capa_Negocio.Inventario_NEG.TablasMysql.EspecificacionesTecnicas_N().BuscarEspTecnica(id);
                if (especificacionTecnica == null || string.IsNullOrEmpty(especificacionTecnica.RutaArchivo))
                    return null;
                string contentType = MimeMapping.GetMimeMapping(especificacionTecnica.RutaArchivo); // Obtiene el tipo MIME
                return File(especificacionTecnica.RutaArchivo, contentType, Path.GetFileName(especificacionTecnica.RutaArchivo));
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }
        public FileResult DocumentoProtocolo(string id, string Lote, int idOperacion = 15)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var user = (Usuario_E)Session["Usuario"];
                bool caracterEspecial = id.Contains("ampersand");
                if (caracterEspecial)
                    id = id.Replace("ampersand", "&");
                var protocolo = new Protocolos_N().BuscarProtocolo(id, Lote);
                if (protocolo == null || string.IsNullOrEmpty(protocolo.RutaArchivo))
                    return null;
                string contentType = MimeMapping.GetMimeMapping(protocolo.RutaArchivo); // Obtiene el tipo MIME
                return File(protocolo.RutaArchivo, contentType, Path.GetFileName(protocolo.RutaArchivo));
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }

        public ActionResult DocumentoProtocoloPDF(string id, string lote)
        {
            var uti = new Utilitarios();
            var protocolo = new Protocolos_N().BuscarProtocolo(id, lote);
            if (protocolo == null || string.IsNullOrEmpty(protocolo.RutaArchivo))
                return Content("No se encontro el archivo");

            string rutaFisica = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), protocolo.Ruta);

            if (!System.IO.File.Exists(rutaFisica))
                return Content("No se encontro el archivo");

            var stream = new FileStream(rutaFisica, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf");
        }

        public ActionResult DocumentoRegSanitarioPDF(string id)
        {
            var uti = new Utilitarios();
            var regsanitarios = new RegistrosSanitarios_N().BuscarRegSanitario(id);
            if (regsanitarios == null || string.IsNullOrEmpty(regsanitarios.RutaArchivo))
                return Content("No se encontró el archivo.");

            string rutaFisica = Path.Combine(uti.directorioFileServer.TrimEnd('/', '\\'), regsanitarios.Ruta);

            if (!System.IO.File.Exists(rutaFisica))
                return Content("No se encontró el archivo.");

            var stream = new FileStream(rutaFisica, FileMode.Open, FileAccess.Read);
            return File(stream, "application/pdf");
        }

        public ActionResult DocumentoEspTecnicaPDF(string id)
        {
            var uti = new Utilitarios();
            var protocolo = new EspecificacionesTecnicas_N().BuscarEspTecnica(id);

            if (protocolo == null || string.IsNullOrEmpty(protocolo.RutaArchivo))
            {
                return new HttpStatusCodeResult(404, "PDF no encontrado");
            }

            string rutaFisica = Path.Combine(
                uti.directorioFileServer.TrimEnd('/', '\\'),
                protocolo.RutaArchivo   // ⚠️ MISMA PROPIEDAD
            );

            if (!System.IO.File.Exists(rutaFisica))
            {
                return new HttpStatusCodeResult(404, "PDF no encontrado");
            }

            var stream = new FileStream(
                rutaFisica,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(stream, "application/pdf");
        }

        public JsonResult ListarArticulosJson()
        {
            var data = new OITM_N().ListarArticulosJson(new OITM_E()); // sin filtros
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult BuscarArticulosAjax(OITM_E filtro)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filtro.ItemCode) &&
                    string.IsNullOrWhiteSpace(filtro.ItemName) &&
                    filtro.FirmCode == 0)
                {
                    return Json(new { error = true, message = "Debe ingresar al menos un criterio de búsqueda." });
                }

                var lista = new OITM_N().ListarArticulosJson(filtro);
                var resultado = lista.Take(200); // puedes paginar aquí si quieres

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { error = true, message = "Error al buscar artículos", detalle = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult BuscarSerialesAjax(RegistrosSanitarios_E filtro)
        {
            var filtroOBTN = new OBTN_E
            {
                MnfSerial = filtro.MnfSerial
            };

            var seriales = new OBTN_N().ObtenerRegistrosSanitariosExistentes(filtroOBTN);

            var resultado = seriales.Select(s => new
            {
                value = s.MnfSerial,
                text = s.MnfSerial
            });

            return Json(resultado);
        }

        public JsonResult SelectLotesArticulo(string ItemCode)
        {
            var lista = new OBTN_N().ListarLotesJson(ItemCode);
            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult VerificarLote(string itemCode, string lote)
        {
            var n = new Protocolos_N();
            bool registrado = n.VerificarLoteExistente(itemCode, lote);
            return Json(new { registrado = registrado }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult VerificarSerial(string mnfserial)
        {
            var n = new RegistrosSanitarios_N();
            bool registrado = n.VerificarSerialExistente(mnfserial);
            return Json(new { registrado = registrado }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult VerificarEspTec(string itemcode)
        {
            var n = new EspecificacionesTecnicas_N();
            bool registrado = n.VerificarEspTecExistente(itemcode);
            return Json(new { registrado = registrado }, JsonRequestBehavior.AllowGet);
        }

        public FileResult Manual(int idOperacion = 2)
        {
            var user = Session["Usuario"] as Usuario_E;

            if (user == null)
            {
                // Para pruebas: esto te confirmará si realmente se perdió la sesión
                throw new Exception("Session['Usuario'] es null en DocumentosController");
            }
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                Utilitarios uti = new Utilitarios();
                Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N oetiN = new Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N();
                //var user = Session["Usuario"] as Usuario_E;
                string ruta = oetiN.BuscarRutaManual(user.TipoUsuarioId);
                ruta = Path.Combine(uti.directorioGeneral, ruta);
                return File(ruta, "application/pdf", "Manual.pdf");
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }

        [HttpPost]
        public JsonResult EliminarProtocoloAjax(string itemCode, string distNumber)
        {
            try
            {
                var protocolosN = new Protocolos_N();

                // 1. Buscar el protocolo en BD para obtener la ruta
                var protocolo = protocolosN.BuscarProtocolo(itemCode, distNumber);

                if (protocolo != null && !string.IsNullOrWhiteSpace(protocolo.RutaArchivo))
                {
                    // Aseguramos que la ruta esté limpia
                    string rutaArchivo = protocolo.RutaArchivo.Trim();

                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        System.IO.File.Delete(rutaArchivo);
                    }
                }

                // 2. Eliminar el registro en la BD
                protocolosN.EliminarProtocolo(itemCode, distNumber);

                return Json(new { success = true, message = "Protocolo y archivo eliminados correctamente." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult EliminarRegSanitarioAjax(string MnfSerial)
        {
            try
            {
                var registrosN = new RegistrosSanitarios_N();

                // 1. Buscar registro para obtener la ruta del archivo
                var registro = registrosN.BuscarRegSanitario(MnfSerial); // Este método debe retornar la entidad con RutaArchivo

                if (registro != null && !string.IsNullOrWhiteSpace(registro.RutaArchivo))
                {
                    string rutaArchivo = registro.RutaArchivo.Trim();

                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        System.IO.File.Delete(rutaArchivo);
                    }
                }

                // 2. Eliminar el registro en la BD
                var resultado = registrosN.EliminarRegSanitario(MnfSerial);

                return Json(new
                {
                    success = resultado.Estado,
                    message = resultado.Mensaje
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }


        [HttpPost]
        public JsonResult EliminarEspTecnicaAjax(string ItemCode)
        {
            try
            {
                var espN = new EspecificacionesTecnicas_N();

                // 1. Buscar la especificación para obtener la ruta del archivo
                var esp = espN.BuscarEspTecnica(ItemCode); // Este método debe retornar la entidad con RutaArchivo

                if (esp != null && !string.IsNullOrWhiteSpace(esp.RutaArchivo))
                {
                    string rutaArchivo = esp.RutaArchivo.Trim();

                    if (System.IO.File.Exists(rutaArchivo))
                    {
                        System.IO.File.Delete(rutaArchivo);
                    }
                }

                // 2. Eliminar el registro en la BD
                var resultado = espN.EliminarEspTecnicas(ItemCode);

                return Json(new
                {
                    success = resultado.Estado,
                    message = resultado.Mensaje
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error: " + ex.Message
                });
            }
        }

        public PartialViewResult FiltrarDocumentos(LotesRegistroSanitario_E filtros)
        {
            var lista = new LotesRegistroSanitario_N().ListarLotesRegistroSanitario(filtros).Item2;
            return PartialView("_TablaDocumentos", lista);
        }

        public PartialViewResult FiltrarComprobantes(Factura_E filtros)
        {
            var usuario = Session["Usuario"] as Usuario_E;

            // Verifica si todos los filtros están vacíos
            bool filtrosVacios = filtros != null &&
                string.IsNullOrWhiteSpace(filtros.CardCode) &&
                string.IsNullOrWhiteSpace(filtros.FechaInicio) &&
                string.IsNullOrWhiteSpace(filtros.FechaFin) &&
                string.IsNullOrWhiteSpace(filtros.Correlativo) &&
                string.IsNullOrWhiteSpace(filtros.Correlativo2);
            // El nombre de usuario es el CardCode
            if (usuario != null && usuario.TipoUsuarioId == 3)
                filtros.CardCode = usuario.Usuario;
            // Si no es cliente, solo asigna CardCode si hay algún filtro activo
            else if (filtros != null && string.IsNullOrWhiteSpace(filtros.CardCode) && !filtrosVacios)
                filtros.CardCode = usuario.Usuario;   

            var lista = new Factura_N().ListarComprobantes(filtros).Item2;
            return PartialView("_TablaComprobantes", lista);
        }

        public PartialViewResult FiltrarProtocolos(string ItemCode, string ItemName,string DistNumber, string OpCarga, DateTime? TiempoCarga)
        {
            var filtro = new Protocolos_E
            {
                ItemCode = ItemCode,
                ItemName = ItemName,
                DistNumber = DistNumber,
                OpCarga = OpCarga,
                TiempoCarga = TiempoCarga ?? DateTime.MinValue
            };
            var negocio = new Protocolos_N();
            var (helper, lista) = negocio.ListarProtocolos(filtro);

            return PartialView("_TablaProtocolos", lista);
        }

        public PartialViewResult FiltrarRegSanitario(string MnfSerial, string ItemName, string OpCarga, DateTime? TiempoCarga)
        {
            var filtro = new RegistrosSanitarios_E
            {
                MnfSerial = MnfSerial,
                ItemName = ItemName,
                OpCarga = OpCarga,
                TiempoCarga = TiempoCarga ?? DateTime.MinValue
            };
            var negocio = new RegistrosSanitarios_N();
            var (helper, lista) = negocio.ListarRegSanitarios(filtro);

            return PartialView("_TablaRegSanitario", lista);
        }

        public PartialViewResult FiltrarEspTecnica(string ItemCode, string ItemName,string Nombre, string OpCarga, DateTime? TiempoCarga)
        {
            var filtro = new EspecificacionesTecnicas_E
            {
                ItemCode = ItemCode,
                ItemName = ItemName,
                Nombre = Nombre,
                OpCarga = OpCarga,
                TiempoCarga = TiempoCarga ?? DateTime.MinValue
            };
            var negocio = new EspecificacionesTecnicas_N();
            var (helper, lista) = negocio.ListarEspTecnicas(filtro);

            return PartialView("_TablaEspTecnica", lista);
        }

        public ActionResult GestionUsuarios()
        {
            return RedirectToAction("GestionUsuarios", "Usuario");
        }

        public ActionResult CerrarSesion()
        {
            return RedirectToAction("CerrarSesion", "Usuario");
        }

        public ActionResult ActualizarContrasena()
        {
            return RedirectToAction("ActualizarContrasena", "Usuario");
        }

    }
}