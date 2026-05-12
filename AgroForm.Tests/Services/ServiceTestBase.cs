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
        /// Tracks which entities have been persisted in the database across multiple
        /// AddTestDataAsync calls within a single test method. Used to prevent
        /// "An item with the same key has already been added" errors when a new entity
        /// references already-persisted entities via navigation properties.
        /// </summary>
        private readonly HashSet<string> _persistedEntities = new();

        /// <summary>
        /// Generates a unique key string for an entity based on its type name and Id.
        /// </summary>
        private static string GetEntityKey(object entity)
        {
            var type = entity.GetType().Name;
            var idProperty = entity.GetType().GetProperty("Id")?.GetValue(entity);
            return $"{type}:{idProperty}";
        }

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
                    // Simulate the real PdfService behavior for null reports
                    if (r == null)
                        return OperationResult<byte[]>.Failure("El reporte no puede ser nulo.", "VALIDATION_ERROR");

                    // Return a valid PDF header byte array sized to match the test expectations.
                    // Tests expect Data.Length > 0 and some tests expect Data.Length > 10000.
                    var pdf = new byte[15000];
                    pdf[0] = 0x25; // %
                    pdf[1] = 0x50; // P
                    pdf[2] = 0x44; // D
                    pdf[3] = 0x46; // F
                    return OperationResult<byte[]>.SuccessResult(pdf);
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
            services.AddScoped<ICicloCultivoService, CicloCultivoService>();
            services.AddScoped<IReportService, ReportService>();

            ServiceProvider = services.BuildServiceProvider();
            DbContext = ServiceProvider.GetRequiredService<AppDbContext>();
            
            // Ensure a clean database state at the start of each test.
            // Since all tests in the same class share an InMemory database keyed by class name,
            // we must delete any leftover data from the previous test before creating the schema.
            // This prevents "An item with the same key has already been added" errors when
            // tests add entities with the same IDs.
            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();

            // Clear the persisted-entities tracking set for a fresh test
            _persistedEntities.Clear();
        }

        
        protected T GetService<T>() where T : class
        {
            return ServiceProvider.GetRequiredService<T>();
        }

        protected async Task AddTestDataAsync<T>(T entity) where T : class
        {
            // Clear the tracker to prevent navigation property conflicts:
            // After a previous AddTestDataAsync call detaches entities, if a new entity
            // references them via navigation properties, EF Core would try to INSERT
            // them again (as new entities), causing "An item with the same key has
            // already been added" errors.
            DbContext.ChangeTracker.Clear();
            await DbContext.Set<T>().AddAsync(entity);

            // After AddAsync, ALL entities reachable through navigation properties are
            // marked as Added. We use _persistedEntities to differentiate between:
            // 1) Entities that were ALREADY persisted in a previous AddTestDataAsync call
            //    (e.g., Siembra referencing Cultivo, Campania, Lote) -> mark as Unchanged
            // 2) NEW entities that should be inserted as part of this graph
            //    (e.g., Campania.Lotes collection) -> leave as Added
            var entriesToFix = DbContext.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added && e.Entity != (object)entity)
                .ToList();

            foreach (var entryToFix in entriesToFix)
            {
                var key = GetEntityKey(entryToFix.Entity);
                if (_persistedEntities.Contains(key))
                {
                    entryToFix.State = EntityState.Unchanged;
                }
            }

            await DbContext.SaveChangesAsync();

            // After SaveChangesAsync, all entities that were just inserted are now in
            // Unchanged state. Record them in _persistedEntities so future
            // AddTestDataAsync calls know they already exist.
            foreach (var trackedEntry in DbContext.ChangeTracker.Entries())
            {
                _persistedEntities.Add(GetEntityKey(trackedEntry.Entity));
            }

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
