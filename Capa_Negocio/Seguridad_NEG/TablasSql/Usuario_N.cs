using Capa_Datos.Seguridad_DAO.TablasSql;
using Capa_Entidad.Seguridad_ENT.TablasSql;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace Capa_Negocio.Seguridad_NEG.TablasSql
{
    public class Usuario_N
    {
        Usuarios_D _datos = new Usuarios_D();
        private bool cadenaVacia(string cad)
        {
            if (cad == null || cad.Replace(" ", "").Length == 0) { return true; }
            else return false;
        }
        private bool validarNuevaContrasena(string plainText)
        {
            bool resultado;
            Regex regex = new Regex(@"^(?=.*?[A-Za-zñÑ0-9])(?=.*?[#&$*@]).{6,12}$");
            Match match = regex.Match(plainText);
            resultado = match.Success;
            return resultado;
        }

        public bool VerificarContrasenaActual(int idUsuario, string contrasenaIngresada)
        {
            Usuarios_D capaDatos = new Usuarios_D();  // Crear instancia
            return capaDatos.ValidarContrasenaActual(idUsuario, contrasenaIngresada);
            
        }
        public Usuario_E BuscarUsuarioLogueo(Usuario_E user)
        {
            return _datos.BuscarUsuarioLogueo(user);
        }
        public List<Usuario_E> ListarUsuarios(Usuario_E filtro)
        {
            return _datos.ListarUsuarios(filtro);
        }
        public List<Usuario_E> ListarUsuariosTipo(Usuario_E filtro, string user)
        {
            return _datos.ListarUsuariosTipo(filtro, user);
        }
        public Usuario_E ObtenerDatosUsuario(int id)
        {
            return _datos.ObtenerDatosUsuario(id);
        }
        public string RegistrarUsuario(Usuario_E obj)
        {
            validarNuevoUsuario(obj);
            return _datos.RegistrarUsuario(obj);
        }
        public string ActualizarContrasena(Usuario_E obj)
        {
            validarActualizarContrasena(obj);
            return _datos.ActualizarContrasena(obj);
        }
        public string ReseteoContrasena(Usuario_E obj)
        {
            return _datos.ReseteoContrasena(obj);
        }
        //validaciones 
        public void validarNuevoUsuario(Usuario_E obj)
        {
            if (obj.TipoUsuarioId <= 0) { throw new Exception("Debe seleccionar tipo"); }
            if (cadenaVacia(obj.Nombre)) { throw new Exception("Ingrese nombre"); }
            if (cadenaVacia(obj.Usuario)) { throw new Exception("Ingrese usuario"); }
            if (cadenaVacia(obj.Contrasena)) { throw new Exception("Ingrese una contraseña"); }
            if (!obj.Contrasena.Equals(obj.NuevaContrasena)) { throw new Exception("Contrasenas no coinciden"); }
        }
        public void validarActualizarContrasena(Usuario_E obj)
        {
            var usuario = ObtenerDatosUsuario(obj.Id);
            if (usuario == null)
                throw new Exception("Volver a iniciar sesión.");
            if (string.IsNullOrWhiteSpace(obj.Contrasena)) { throw new Exception("Ingrese su contraseña actual"); }
            Usuarios_D capaDatos = new Usuarios_D(); // instancia capa datos
            bool esContrasenaCorrecta = capaDatos.ValidarContrasenaActual(obj.Id, obj.Contrasena);
            if (!esContrasenaCorrecta)
                throw new Exception("Verificar la contraseña actual ingresada");
            if (cadenaVacia(obj.Contrasena)) { throw new Exception("Ingrese su contraseña actual"); }
            if (cadenaVacia(obj.NuevaContrasena)) { throw new Exception("Ingrese su nueva contraseña"); }
            if (cadenaVacia(obj.NuevaContrasena2)) { throw new Exception("Repita su nueva contraseña"); }
            if (validarNuevaContrasena(obj.NuevaContrasena) == false) { throw new Exception("La contraseña ingresada no cumple con los requisitos"); }
            if (obj.NuevaContrasena.Equals(obj.NuevaContrasena2) == false) { throw new Exception("Las contraseñas ingresadas no coinciden"); }
        }
    }
}
