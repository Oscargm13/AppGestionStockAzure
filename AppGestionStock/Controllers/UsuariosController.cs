using AppGestionStock.Extensions;
using AppGestionStock.Models;
using AppGestionStock.Repositories;
using AppGestionStock.Services;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            string token = await service.GetTokenAsync(email, pass);
            Usuario usuario = await repo.CompararUsuario(email, pass);
            if (token != null)
            {
                HttpContext.Session.SetObject("TOKEN", token);
                HttpContext.Session.SetObject("USUARIO", usuario);
                HttpContext.Session.SetObject("EMAIL", usuario.Email);
                HttpContext.Session.SetObject("IDUSUARIO", usuario.IdUsuario);

                return RedirectToAction("Index", "Home");
            }
            

            ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
            return View();
        }
        //public async Task<IActionResult> LogIn(string nombre, string pass)
        //{
        //    Usuario usuario = await repo.CompararUsuario(nombre, pass);

        //    if (usuario != null)
        //    {
        //        HttpContext.Session.SetObject("USUARIO", usuario);
        //        HttpContext.Session.SetObject("EMAIL", usuario.Email);
        //        HttpContext.Session.SetObject("IDUSUARIO", usuario.IdUsuario);

        //        return RedirectToAction("Index", "Home");
        //    }

        //    ViewData["MensajeError"] = "Nombre de usuario o contraseña incorrectos.";
        //    return View();
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
