using Microsoft.AspNetCore.Mvc;

namespace AppGestionStock.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraficosController : ControllerBase
    {
        [HttpGet("datosCircular")]
        public IActionResult ObtenerDatosCircular()
        {
            var datos = new[]
            {
               new { label = "Lacteos", valor = 30 },
               new { label = "Frutas", valor = 50 },
               new { label = "Cereales", valor = 20 }
           };
            return Ok(datos);
        }
    }
}
