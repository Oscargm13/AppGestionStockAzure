using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AppGestionStock.Filters
{
    public class AuthorizeUsuariosAttribute: AuthorizeAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity.IsAuthenticated == false)
            {
                RouteValueDictionary routeLogin =
                new RouteValueDictionary(new
                {
                    controller = "Usuarios",
                    action = "LogIn"
                });
                context.Result = new
                RedirectToRouteResult(routeLogin);
            }
        }
    }
}
