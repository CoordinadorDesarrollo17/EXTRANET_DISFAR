using Capa_Entidad.Seguridad_ENT.TablasSql;
using Capa_Entidad.SociosNegocios_ENT.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
using Capa_Negocio;
using Capa_Negocio.SociosNegocios_NEG.TablasSql;
using Capa_Negocio.Transacciones_NEG.TablasHana;
using Capa_Negocio.Transacciones_NEG.TablasSql;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Capa_Usuario.Controllers
{
    public class UsuarioController : Controller
    {
        OINV_N _negFacturasHana = new OINV_N();
        Factura_N _negFacturas = new Factura_N();

        // Variable estática para almacenar intentos fallidos
        private static Dictionary<string, (int intentos, DateTime ultimoIntento)> intentosFallidos = new Dictionary<string, (int, DateTime)>();

        [HttpPost]
        public ActionResult Logueo(Usuario_E user, int idOperacion = 1)
        {
            string ip = Request.UserHostAddress;
            int intentosMaximos = 5; 
            int tiempoBloqueoMinutos = 10;

            // Verificar si está bloqueado
            if (intentosFallidos.ContainsKey(ip))
            {
                var datos = intentosFallidos[ip];
                if (datos.intentos >= intentosMaximos &&
                    (DateTime.Now - datos.ultimoIntento).TotalMinutes < tiempoBloqueoMinutos)
                {
                    int minutosRestantes = tiempoBloqueoMinutos - (int)(DateTime.Now - datos.ultimoIntento).TotalMinutes;
                    return RedirectToAction("Index","Home", new
                    {
                        msj = $"Demasiados intentos fallidos. Intente de nuevo en {minutosRestantes} minutos."
                    });
                }
            }

            var ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();

            try
            {
                var usuario = ousrN.BuscarUsuarioLogueo(user);
                if (usuario == null)
                {
                    // Registrar intento fallido
                    if (intentosFallidos.ContainsKey(ip))
                        intentosFallidos[ip] = (intentosFallidos[ip].intentos + 1, DateTime.Now);
                    else
                        intentosFallidos[ip] = (1, DateTime.Now);

                    int intentosActuales = intentosFallidos[ip].intentos;
                    int intentosRestantes = intentosMaximos - intentosActuales;

                    if (intentosRestantes > 0)
                    {
                        return RedirectToAction("Index", "Home", new
                        {
                            msj = $"Usuario o contraseña no válidos. Intento {intentosActuales} de {intentosMaximos}."
                        });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new
                        {
                            msj = $"Cuenta bloqueada por {tiempoBloqueoMinutos} minutos debido a múltiples intentos fallidos."
                        });
                    }
                }
                // Si entra, limpiar contador

                if (intentosFallidos.ContainsKey(ip))
                    intentosFallidos.Remove(ip);

                Session["Usuario"] = usuario;
                return RedirectToAction("SesionUsuario");
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = ex.Message;
                TempData.Keep("Mensaje");
                return RedirectToAction("Index","Home");
            } 
            
        }

        public ActionResult SesionUsuario(string msj, int idOperacion = 2)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var user = (Usuario_E)Session["Usuario"];
                if (user == null)
                {
                    return RedirectToAction("Index","Home", new { msj = "La sesión ha expirado." });
                }

                if (user.TipoUsuarioId.Equals(3))
                {
                    Task.Run(() =>
                    {
                        List<int> docEntrys = _negFacturasHana.ListarDocEntryComprobantes(user.Usuario);
                        foreach (var docEntry in docEntrys)
                        {
                            var comprobanteExistente = _negFacturas.ListarComprobantes(new Factura_E { DocEntry = docEntry }).Item2.FirstOrDefault();
                            if (comprobanteExistente == null || comprobanteExistente.DocEntry == 0)
                            {
                                var obj = _negFacturasHana.BuscarComprobante(docEntry);
                                if (obj != null && obj.DocEntry > 0)
                                {
                                    _negFacturasHana.ImportarComprobante(obj);
                                }
                            }
                        }
                    });
                }

                ViewBag.mensaje = msj;
                ViewBag.TipoUsuarioSesion = user.TipoUsuarioId;
                ViewBag.NombreUsuario = user.Nombre;

                // SOLUCIÓN: Pasar el modelo a la vista
                return View(new List<Usuario_E> { user });
            }
            else if (acceso == "E_Login")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorOperacion","Home");
            }
        }

        public ActionResult CerrarSesion()
        {
            Session.Remove("Usuario");
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GestionUsuarios(Usuario_E filtro, int idOperacion = 7)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                var tipoUsuarioNeg = new Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N();
                Usuario_E user = (Usuario_E)Session["Usuario"];

                var usuarios = ousrN.ListarUsuariosTipo(filtro, user.Usuario);

                ViewBag.Tipos = tipoUsuarioNeg.ListaTiposUsuario();
                ViewBag.fil = filtro;

                

                return View(usuarios);
            }
            else if (acceso == "E_Login")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return RedirectToAction("ErrorOperacion","Home");
            }
        }

        [HttpGet]
        public ActionResult NuevoUsuario(int idOperacion = 8)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                var tipoUsuarioNeg = new Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N();
                ViewBag.Tipos = tipoUsuarioNeg.ListaTiposUsuario();
                return View(); // Asegúrate de tener una vista NuevoUsuario.cshtml
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

        [HttpPost]
        public ActionResult NuevoUsuario(Usuario_E obj, int idOperacion = 8)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);

            if (acceso == "C_Access")
            {
                Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N oetiN = new Capa_Negocio.Seguridad_NEG.TablasSql.TipoUsuario_N();
                Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                try
                {
                    var user = Session["Usuario"] as Usuario_E;
                    obj.OpCreacion = user.Nombre;
                    ousrN.RegistrarUsuario(obj);
                    return RedirectToAction("GestionUsuarios");
                }
                catch (Exception e)
                {
                    ViewBag.Mensaje = e.Message;
                    ViewBag.Tipos = oetiN.ListaTiposUsuario();
                    return View(obj);
                }
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

        public ActionResult ActualizarContrasena(int idOperacion = 11)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            //REVISADO
            if (acceso == "C_Access")
            {
                var usuario = Session["Usuario"] as Usuario_E;
                return View(usuario);
            }
            else if (acceso == "E_Login")
            { return RedirectToAction("Index", "Home"); }
            else
            { return RedirectToAction("ErrorOperacion", "Home"); }
        }
        [HttpPost]
        public ActionResult ActualizarContrasena(Usuario_E obj, int idOperacion = 11)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                try
                {
                    var user = Session["Usuario"] as Usuario_E;
                    obj.Usuario = user.Usuario;
                    new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N().ActualizarContrasena(obj);
                    return RedirectToAction("SesionUsuario", new { msj = "Contraseña Actualizada" });
                }
                catch (Exception e)
                { ViewBag.Mensaje = e.Message; return View(obj); }
            }
            else if (acceso == "E_Login")
            {
                return RedirectToAction("Index", "Home");
            }
            else
            { return RedirectToAction("ErrorOperacion", "Home"); }
        }

        [HttpGet]
        public ActionResult ResetearContraseña(int id, int idOperacion = 12)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso != "C_Access")
                return RedirectToAction("ErrorOperacion", "Home");

            try
            {
                var ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                var obj = ousrN.ObtenerDatosUsuario(id);

                if (obj == null)
                {
                    ViewBag.Mensaje = "Usuario no encontrado.";
                    return View("GestionUsuarios");
                }

                ousrN.ReseteoContrasena(obj);

                return View(obj); // muestra la nueva vista con los datos
            }
            catch (Exception ex)
            {
                ViewBag.Mensaje = ex.Message;
                return View("GestionUsuarios");
            }
        }

        [HttpPost]
        public JsonResult ObtenerDatosResetearContraseña(int id)
        {
            try
            {
                var ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                var obj = ousrN.ObtenerDatosUsuario(id);

                if (obj == null)
                    return Json(new { error = "Usuario no encontrado." });

                ousrN.ReseteoContrasena(obj);

                return Json(new
                {
                    obj.NombreTipoUsuario,
                    obj.Usuario,
                    obj.Nombre,
                    obj.NuevaContrasena
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
        public JsonResult ListarUsuariosClientes(int idOperacion = 8)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                List<Clientes_E> obj = new Clientes_N().ListarClientes(null, false);
                return Json(obj);
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }
        public ActionResult validarNuevoUsuario(Usuario_E obj, int idOperacion = 8)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                string status = "true";
                try
                {
                    ousrN.validarNuevoUsuario(obj);
                    return Content(status);
                }
                catch (Exception e) { return Content(e.Message); }
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }
        public ActionResult validarNuevaContrasena(Usuario_E obj, int idOperacion = 11)
        {
            string acceso = Helpers.VerificacionAccesos(this, idOperacion);
            if (acceso == "C_Access")
            {
                Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N ousrN = new Capa_Negocio.Seguridad_NEG.TablasSql.Usuario_N();
                string status = "true";
                try
                {
                    ousrN.validarActualizarContrasena(obj);
                    return Content(status);
                }
                catch (Exception e) { return Content(e.Message); }
            }
            else if (acceso == "E_Login") { return null; }
            else { return null; }
        }
        
        public ActionResult GestionDocumentos()
        {
            return RedirectToAction("GestionDocumentos", "Documentos");
        }

        public ActionResult GestionComprobantes()
        {
            return RedirectToAction("GestionComprobantes", "Documentos");
        }

        public ActionResult ConsultarComprobantes()
        {
            return RedirectToAction("ConsultarComprobantes", "Documentos");
        }
    }
}