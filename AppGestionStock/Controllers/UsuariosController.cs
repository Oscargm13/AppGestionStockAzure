using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;

namespace AppGestionStock.Controllers
{
    public class UsuariosController : Controller
    {
        private RepositoryUsuario repo;

        public UsuariosController(RepositoryUsuario repo)
        {
            this.repo = repo;
        }

        public async Task<IActionResult> LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(string nombre, string pass)
        {
            Usuario usuario = await repo.CompararUsuario(nombre, pass);

            if (usuario != null)
            {
                HttpContext.Session.SetObject("USUARIO", usuario);
                HttpContext.Session.SetObject("EMAIL", usuario.Email);
                HttpContext.Session.SetObject("IDUSUARIO", usuario.IdUsuario);
                
                return RedirectToAction("Index", "Home");
            }

            ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
            return View();
        }

        public async Task<IActionResult> LogOut()
        {
            if(HttpContext.Session.GetObject<Usuario>("USUARIO") != null)
            {
                HttpContext.Session.Remove("USUARIO");
            }

            return RedirectToAction("LogIn", "Usuarios");
        }
    }
}
