﻿
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Log.Error($"- Environment: {environment}");

            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .Build();

            var builder = WebApplication.CreateBuilder(args);

            // Configuración de Serilog
            var logsDirectory = Path.Combine(builder.Environment.ContentRootPath, "logs");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(logEvent =>
                    logEvent.Exception is UnauthorizedAccessException)
                .WriteTo.File(
                    Path.Combine(logsDirectory, "logfile.txt"),
                    rollingInterval: RollingInterval.Month,
                    retainedFileCountLimit: 3,
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning
                )
                .CreateLogger();

            builder.Host.UseSerilog();

            // ✅ CONFIGURACIÓN DE IDENTITY (COMPLETA)
            //builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            //{
            //    // Configuración de password
            //    options.Password.RequireDigit = true;
            //    options.Password.RequiredLength = 6;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequireUppercase = false;
            //    options.Password.RequireLowercase = true;

            //    // Configuración de usuario
            //    options.User.RequireUniqueEmail = true;
            //    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            //    // Configuración de lockout
            //    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            //    options.Lockout.MaxFailedAccessAttempts = 5;
            //    options.Lockout.AllowedForNewUsers = true;

            //    // Configuración de sign-in
            //    options.SignIn.RequireConfirmedEmail = false;
            //    options.SignIn.RequireConfirmedPhoneNumber = false;
            //    options.SignIn.RequireConfirmedAccount = false;
            //})
            //.AddEntityFrameworkStores<AppDbContext>()
            //.AddDefaultTokenProviders()
            //.AddErrorDescriber<IdentityErrorDescriber>();

            builder.Services.AddAuthentication("AgroFormAuth")
                .AddCookie("AgroFormAuth", options =>
                {
                    options.Cookie.Name = "AgroFormAuth";
                    options.LoginPath = "/Access/Login";
                    options.LogoutPath = "/Access/Logout";
                    options.AccessDeniedPath = "/Access/AccessDenied";
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(180);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Strict;
                });

            // Configuración de servicios
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
                .SetApplicationName("AgroForm");

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages(); // ✅ Agregar para páginas de Identity
            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            builder.Services.AddCors(_ =>
            {
                _.AddPolicy("NuevaPolitica", app =>
                {
                    app.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Configuración de DbContext
            //builder.Services.AddDbContext<AppDbContext>(options =>
            //{
            //    options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"), sqlServerOptionsAction: sqlOptions =>
            //    {
            //        sqlOptions.CommandTimeout(60);
            //        sqlOptions.EnableRetryOnFailure(
            //            maxRetryCount: 5,
            //            maxRetryDelay: TimeSpan.FromSeconds(30),
            //            errorNumbersToAdd: null)
            //        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            //    });
            //});
            builder.Services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"), sqlOptions =>
                {
                    sqlOptions.CommandTimeout(60);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)
                              .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });


            // Servicios de aplicación
            builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddApplicationServices();

            // Configuración cultural
            var cultureInfo = new CultureInfo("es-ES");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN";
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Configuración adicional de DataProtection
            var keysPath = Path.Combine(Directory.GetCurrentDirectory(), "keys");
            try
            {
                if (!Directory.Exists(keysPath))
                {
                    Directory.CreateDirectory(keysPath);
                }

                builder.Services.AddDataProtection()
                    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
                    .SetApplicationName("AgroForm")
                    .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
            }
            catch (Exception e)
            {
                Log.Error(e, $"ERROR EN LA KEY, path: {keysPath}. Error: {e.Message}");
            }

            // ✅ CONFIGURACIÓN HTTPS
            builder.Services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                options.HttpsPort = 443;
            });

            var app = builder.Build();

            // 1. Middlewares de configuración inicial
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();
            app.UseCors("NuevaPolitica");

            // 2. Middlewares de redirección (antes de routing)
            app.Use(async (context, next) =>
            {
                var host = context.Request.Host.Value.ToLower();
                if (!host.StartsWith("www."))
                {
                    var newUrl = $"{context.Request.Scheme}://www.{host}{context.Request.Path}{context.Request.QueryString}";
                    context.Response.Redirect(newUrl, permanent: true);
                    return;
                }
                await next();
            });

            // Manejo de errores centralizado
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler(errorApp =>
                {
                    errorApp.Run(async context =>
                    {
                        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                        if (exception is UnauthorizedAccessException)
                        {
                            context.Response.Redirect("/Access/Login");
                            return;
                        }

                        Log.Error(exception, "Error no manejado");

                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.ContentType = "text/html";
                        await context.Response.WriteAsync(@"
                            <html>
                                <head><title>Error</title></head>
                                <body>
                                    <h1>Ocurrió un error en el servidor.</h1>
                                    <p>Por favor, intente nuevamente más tarde.</p>
                                </body>
                            </html>");
                    });
                });
            }

            app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

            // 3. Middlewares de rutas estáticas
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
                    ctx.Context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                }
            });

            // 4. Middlewares de seguridad y autenticación (ORDEN CRÍTICO)
            app.UseRouting();

            // ✅ ORDEN CORRECTO: Authentication -> Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithReExecute("/404");

            // 5. Endpoints
            app.MapControllerRoute(
                name: "404",
                pattern: "404",
                defaults: new { controller = "Home", action = "Error404" }
            );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Access}/{action=Login}/{id?}"
            );

            app.MapRazorPages(); // ✅ Para páginas de Identity

            // Endpoint de health check básico
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }));

            Log.Information("Aplicación iniciándose...");
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error crítico durante el inicio de la aplicación");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
