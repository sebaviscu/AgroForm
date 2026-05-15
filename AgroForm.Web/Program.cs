using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Web.Utilities;
using AgroForm.Model;
using AgroForm.Business.Contracts;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Serilog;
using System.Globalization;
using System.Text.Json.Serialization;
using Mapster;

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
                .WriteTo.Console()
                .CreateLogger();

            builder.Host.UseSerilog();

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
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            // Configuración de servicios
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "keys")))
                .SetApplicationName("AgroForm");

            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSignalR();
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddControllers().AddJsonOptions(x =>
                x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // Session (usada para simulación de licencia y otros estados)
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".AgroForm.Session";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromHours(3);
            });

            builder.Services.AddCors(_ =>
            {
                _.AddPolicy("NuevaPolitica", app =>
                {
                    app.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SQL"), sqlOptions =>
                {
                    sqlOptions.CommandTimeout(60);
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)
                              .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });
            });

            // Servicios de aplicación
            // Configuración de Mapster
            builder.Services.AddMapster();
            MapsterConfig.Configure();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddApplicationServices();

            // ==========================================
            // Configuración de Copernicus Data Space Ecosystem (FASE 0)
            // ==========================================
            builder.Services.Configure<CopernicusConfig>(
                builder.Configuration.GetSection("Copernicus"));

            // HttpClient resiliente para Copernicus con Polly
            // La Processing API puede tomar más tiempo que WMTS (hasta 60s)
            // Estrategia: retry (3 intentos, backoff 2s/4s/8s) + circuit breaker (5 fallos, 30s)
            builder.Services.AddHttpClient("Copernicus", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(60);
            })
            .AddTransientHttpErrorPolicy(p => p
                .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))))
            .AddTransientHttpErrorPolicy(p => p
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));

            // Asegurar que el directorio de caché de tiles existe
            var tilesCachePath = builder.Configuration.GetValue<string>("Copernicus:TileCacheRoot") ?? "satelite-tiles";
            if (!Directory.Exists(tilesCachePath))
            {
                Directory.CreateDirectory(tilesCachePath);
            }

            // Configuración cultural
            var cultureInfo = new CultureInfo("es-AR");
            cultureInfo.NumberFormat.NumberDecimalSeparator = ",";
            cultureInfo.NumberFormat.NumberGroupSeparator = ".";
            cultureInfo.NumberFormat.CurrencyDecimalSeparator = ",";
            cultureInfo.NumberFormat.CurrencyGroupSeparator = ".";
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "X-CSRF-TOKEN";
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            builder.Services.AddResponseCaching(); // Requerido por [ResponseCache] con VaryByQueryKeys (tiles satelitales)

            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Configuración HTTPS para desarrollo
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
                    options.HttpsPort = 5001; // ← Puerto de desarrollo
                });
            }
            else
            {
                builder.Services.AddHttpsRedirection(options =>
                {
                    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect;
                    options.HttpsPort = 443; // ← Puerto producción
                });
            }

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

            var app = builder.Build();

            // 1. Middlewares de configuración inicial
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // ✅ ORDEN CORRECTO: HttpsRedirection ANTES de StaticFiles
            app.UseHttpsRedirection();
            app.UseCors("NuevaPolitica");
            app.UseResponseCaching(); // Requerido por [ResponseCache] con VaryByQueryKeys
            app.UseResponseCompression();

            // ❌ ELIMINAR middleware de redirección a www (causa problemas con SSL)
            /*
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
            */

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

            // 4. Middlewares de seguridad y autenticación
            app.UseRouting();
            app.UseSession();
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

            app.MapRazorPages();

            // Endpoint de health check básico
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = TimeHelper.GetArgentinaTime() }));


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