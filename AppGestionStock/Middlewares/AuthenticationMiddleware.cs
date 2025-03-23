namespace AppGestionStock.Middlewares
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (!context.Session.Keys.Contains("IDUSUARIO")) // Verifica si el usuario está autenticado
            {
                if (!context.Request.Path.StartsWithSegments("/Usuarios/LogIn")) // Evita bucles de redirección
                {
                    context.Response.Redirect("/Usuarios/LogIn"); // Redirige al login
                    return;
                }
            }

            await next(context);
        }
    }
}
