using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class UsuariosController : Controller
    {
        private ServiceAlmacenes service;
        
        public UsuariosController(ServiceAlmacenes service)
        {
            this.service = service;
        }

        public async Task<IActionResult> LogIn()
        {
            if(HttpContext.Session.GetObject<Usuario>("TOKEN")!= null)
            {
                ViewData["TOKEN"] = HttpContext.Session.GetObject<Usuario>("TOKEN");
            }
            
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(string email, string pass)
        {
            try
            {
                string token = await service.GetTokenAsync(email, pass);

                if (string.IsNullOrEmpty(token))
                {
                    ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
                    return View();
                }

                HttpContext.Session.SetString("TOKEN", token);

                Usuario usuario = await this.service.FindUsuarioEmailAsync(email);

                HttpContext.Session.SetInt32("IDUSUARIO", usuario.IdUsuario);
                HttpContext.Session.SetString("EMAIL", usuario.Email);
                HttpContext.Session.SetString("NOMBRE", usuario.Nombre);
                HttpContext.Session.SetObject("USUARIO", usuario);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewData["MensajeError"] = "Error al iniciar sesión. Inténtalo de nuevo.";
                return View();
            }
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
