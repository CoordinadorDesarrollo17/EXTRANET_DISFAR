using Capa_Entidad.Seguridad_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Capa_Negocio.Transacciones_NEG.TablasSql;
using Capa_Entidad.Inventario_ENT.TablasSql;
using Capa_Negocio.Inventario_NEG.TablasSql;
using System.IO;
using System.Web;
using Capa_Negocio.SociosNegocios_NEG.TablasSql;
using Capa_Entidad.Transacciones_ENT.TablasSql;
using Capa_Entidad.SociosNegocios_ENT.TablasSql;
using Capa_Negocio.Transacciones_NEG.TablasHana;
using System.Threading.Tasks;
using Capa_Datos;
using PagedList;
namespace Capa_Usuario.Controllers
{
    public class HomeController : Controller
    {
        OINV_N _negFacturasHana = new OINV_N();
        Factura_N _negFacturas = new Factura_N();
        public ActionResult Index(string msj = "")
        {
            ViewBag.Mensaje = msj;
            return View();
        }

        
        public ActionResult ErrorOperacion()
        {
            return View();
        }

    }
}