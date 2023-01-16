using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Phahung_BN.Middleware;
using Phahung_BN.Services;
using Phahung_BN.Controllers;


namespace Phahung_BN
{
    public class Startup
    {

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            System.Console.WriteLine("************************************************************HELLO FROM Startup");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:3021", "http://localhost:3000")
                                            .AllowAnyHeader()
                                            .AllowAnyMethod()
                                            .AllowCredentials()
                                            .SetIsOriginAllowed(origin => true);
                    });
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.AddCors();
            services.AddControllersWithViews();
            services.AddSingleton<FiresbaseConnect>();
            services.AddAuthentication();
            System.Console.WriteLine("************************************************************HELLO FROM ConfigureServices");
            // For Authentication
            services.AddMvc().AddSessionStateTempDataProvider();
            services.AddSession();
            services.AddHttpContextAccessor();
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<usersController>();



        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            
            app.UseRouting();
            app.UseCors();
            // For Authentication
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseSession();
            app.UseMiddleware<AuthMiddleware>();

            

            //app.Use(async (context, next) =>
            //{
            //    //var requestReader = new StreamReader(context.Request.Body);
            //    //var requestContent = requestReader.ReadToEnd();

            //    var requestHeader = context.Request.Headers;
            //    ////var headerString = JsonConvert.SerializeObject(requestHeader);
            //    string token = requestHeader["Token"];
            //    if (token != null)
            //    {
            //        //    Console.WriteLine(token);
            //        FirebaseToken decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            //        context.Items["Uid"] = decodedToken.Uid;
            //        //await context.Response.WriteAsync("\nREQ HEADER \n Token = " + token + "\nREQ BODY ");
            //    }
            //    await next.Invoke();
            //    //Console.WriteLine(token.GetType());

            //});

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            
        }
    }
}
