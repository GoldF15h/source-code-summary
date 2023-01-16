using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Firebase.Auth;
using FirebaseAdmin.Auth;
using FirebaseAdmin;
using Phahung_BN.Services;
using Phahung_BN.Models;


using Newtonsoft.Json;


namespace Phahung_BN.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IUserService _userService;
        public AuthMiddleware(RequestDelegate next, IUserService userService)
        {
            _next = next;
            _userService = userService;
        }

        public async Task Invoke(HttpContext context)
        {
            Console.WriteLine(context.Request.Path + "  @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            if (context.Request.Path == "/" || context.Request.Path == "/signin" || context.Request.Path == "/signup" || context.Request.Path == "/categories")
            {
                await _next(context);
            }
            else
            {

                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                if (token != null)
                {
               
                    try
                    {
                        FirebaseToken decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

                        PrivateUserModel userInfo = await _userService.GetUserInfo(decodedToken.Uid);

                        context.Items["User"] = userInfo; 
                        await _next(context);
                    }
                
                
                     catch(Exception ex)
                    {
                        var result = new ResponseModel("Unauthorized", 401);
                        Console.WriteLine("Hello from Middle ware ---> "+ex.ToString());
                        var json = JsonConvert.SerializeObject(result);
                        await context.Response.WriteAsync(json);
                    }
                    

                }
                else
                {
                    if (context.Request.Method == "GET")
                    {
                        await _next(context);
                    }
                    else
                    {
                        var result = new ResponseModel("Unauthorized", 401);
                        Console.WriteLine("Hello from Middle ware ---> " + "token is null and req is not GET mothod");
                        var json = JsonConvert.SerializeObject(result);
                        await context.Response.WriteAsync(json);
                    }
                
                }
                
            }   // Extension method used to add the middleware to the HTTP request pipeline.
        }
            


    }

    public static class HttpContextItemsMiddlewareExtensions
    {
        public static IApplicationBuilder
            UseHttpContextItemsMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AuthMiddleware>();
        }
    }

}
