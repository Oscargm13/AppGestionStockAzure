using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    public class UsuariosController : Controller
    {
        private RepositoryAlmacen repo;
        private ServiceAlmacenes service;
        
        public UsuariosController(RepositoryAlmacen repo, ServiceAlmacenes service)
        {
            this.repo = repo;
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
                // Obtenemos el token de la API
                string token = await service.GetTokenAsync(email, pass);

                if (string.IsNullOrEmpty(token))
                {
                    ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
                    return View();
                }

                // Guardamos en sesión el token plano (sin serializar)
                HttpContext.Session.SetString("TOKEN", token);

                // Obtenemos datos del usuario
                Usuario usuario = await this.service.FindUsuarioEmailAsync(email);

                // Guardamos solo lo necesario (evita duplicidad de info)
                HttpContext.Session.SetInt32("IDUSUARIO", usuario.IdUsuario);
                HttpContext.Session.SetString("EMAIL", usuario.Email);
                HttpContext.Session.SetString("NOMBRE", usuario.Nombre);
                HttpContext.Session.SetObject("USUARIO", usuario); // solo si luego lo usás completo

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                // 👀 Buen lugar para loguear
                ViewData["MensajeError"] = "Error al iniciar sesión. Inténtalo de nuevo.";
                return View();
            }
        }


        //[HttpPost]
        //public async Task<IActionResult> LogIn(string email, string pass)
        //{
        //    try
        //    {
        //        // Obtener token
        //        string token = await service.GetTokenAsync(email, pass);

        //        if (token == null)
        //        {
        //            ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
        //            return View();
        //        }

        //        // Obtener datos del usuario autenticado desde la API
        //        Usuario usuario = await service.LoginUsuarioAsync(email, pass);

        //        // Guardar en sesión
        //        HttpContext.Session.SetObject("TOKEN", token);
        //        HttpContext.Session.SetObject("USUARIO", usuario);
        //        HttpContext.Session.SetObject("EMAIL", usuario.Email);
        //        HttpContext.Session.SetObject("IDUSUARIO", usuario.IdUsuario);

        //        return RedirectToAction("Index", "Home");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewData["MensajeError"] = $"Error al iniciar sesión: {ex.Message}";
        //        return View();
        //    }
        //}

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
