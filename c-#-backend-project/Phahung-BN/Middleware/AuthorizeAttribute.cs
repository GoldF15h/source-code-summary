using Phahung_BN.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Phahung_BN.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly roles[] allowedRoles;

        public AuthorizeAttribute(params roles[] roles)
        {
            allowedRoles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var account = (PrivateUserModel)context.HttpContext.Items["User"];
            if (account == null)
            {
                // not logged in
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
            else
            {
                bool authorized = false; 
                foreach (roles role in allowedRoles)
                {
                    if (account.role == role)
                    {
                        authorized = true;
                        break;
                    }
                }

                if (!authorized)
                {
                    context.HttpContext.Response.Headers.Add("AuthStatus", "UnAuthorized");
                    context.Result = new JsonResult(new { message = "Unauthorized Role" }) { StatusCode = StatusCodes.Status401Unauthorized };
                }
            }
        }
    }
}