using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using AgroForm.Data.DBContext;
using AgroForm.Data.Repository;
using AgroForm.Model.Configuracion;
using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AgroForm.Model;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public abstract class ServiceTestBase : IDisposable
    {
        protected ServiceProvider ServiceProvider { get; private set; }
        protected AppDbContext DbContext { get; private set; }
        protected UserAuth TestUserAuth { get; private set; } = new UserAuth
        {
            UserName = "testuser",
            IdLicencia = 1,
            IdCampaña = 1,
            IdUsuario = 1,
            IdRol = AgroForm.Model.EnumClass.Roles.Administrador,
            Moneda = AgroForm.Model.EnumClass.Monedas.Peso
        };

        /// <summary>
        /// Automatically generates a unique database name per test class
        /// to prevent data leakage between test classes when using InMemory database.
        /// </summary>
        protected virtual string DatabaseName => GetType().Name;

        protected ServiceTestBase()
        {
            var services = new ServiceCollection();

            // Usar el mismo nombre de base de datos para todos los contextos
            var databaseName = DatabaseName;

            // Configurar DbContext en memoria
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: databaseName)
                       .EnableSensitiveDataLogging());

            // Configurar IDbContextFactory con el mismo nombre
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: databaseName)
                       .EnableSensitiveDataLogging());

            // Configurar logging mock para DbContext
            var dbContextLoggerMock = new Mock<ILogger<AppDbContext>>();
            services.AddSingleton(dbContextLoggerMock.Object);

            // Configurar logging genérico para todos los servicios
            services.AddLogging(builder => builder.ClearProviders().AddConsole());

            // Configurar HttpContextAccessor real con claims configurados
            var httpContext = new DefaultHttpContext();
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim("Licencia", "1"),
                new Claim("Campania", "1"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "1"),
                new Claim("Moneda", "1")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthentication");
            httpContext.User = new ClaimsPrincipal(identity);
            
            services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext });

            // Registrar IUserContext (usando la implementación real que usa IHttpContextAccessor)
            services.AddScoped<IUserContext, AgroForm.Web.Utilities.UserContext>();

            // Configurar UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Registrar mocks para dependencias externas
            var pdfServiceMock = new Mock<IPdfService>();
            pdfServiceMock.Setup(x => x.GenerarPdfCierreCampaniaAsync(It.IsAny<ReporteCierreCampania>()))
                .ReturnsAsync((ReporteCierreCampania r) =>
                {
                    // Return a simple success with a valid PDF header byte array
                    return OperationResult<byte[]>.SuccessResult(new byte[] { 0x25, 0x50, 0x44, 0x46, 0x00 });
                });
            services.AddScoped<IPdfService>(_ => pdfServiceMock.Object);

            // Registrar todos los servicios
            services.AddScoped<IActividadService, ActividadService>();
            services.AddScoped<ICampaniaService, CampaniaService>();
            services.AddScoped<ICampoService, CampoService>();
            services.AddScoped<ICatalogoService, CatalogoService>();
            services.AddScoped<ICierreCampaniaService, CierreCampaniaService>();
            services.AddScoped<ICultivoService, CultivoService>();
            services.AddScoped<IEstadoFenologicoService, EstadoFenologicoService>();
            services.AddScoped<IGastoService, GastoService>();
            services.AddScoped<ILicenciaService, LicenciaService>();
            services.AddScoped<ILoteService, LoteService>();
            services.AddScoped<IMonedaService, MonedaService>();
            services.AddScoped<IRegistroClimaService, RegistroClimaService>();
            services.AddScoped<ITipoActividadService, TipoActividadService>();
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IVariedadService, VariedadService>();
            services.AddScoped<ICicloCultivoService, CicloCultivoService>();
            services.AddScoped<IReportService, ReportService>();

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Crear la base de datos
            DbContext.Database.EnsureCreated();
        }

        
        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        protected async Task AddTestDataAsync<T>(T entity) where T : class
        {
            await DbContext.Set<T>().AddAsync(entity);
            await DbContext.SaveChangesAsync();
            // Detach to prevent tracking conflicts when services use AsNoTracking + _context.Update
            DbContext.Entry(entity).State = EntityState.Detached;
        }

        protected async Task ClearDatabaseAsync()
        {
            // Eliminar todos los datos de la base de datos en memoria
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();
            
            // Limpiar el change tracker
            DbContext.ChangeTracker.Clear();
        }

        public virtual void Dispose()
        {
            DbContext?.Database?.EnsureDeleted();
            DbContext?.Dispose();
            ServiceProvider?.Dispose();
        }
    }
}
