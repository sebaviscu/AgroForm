using AgroForm.Business.Services;
using AgroForm.Data.DBContext;
using Microsoft.Extensions.Logging;

namespace AgroForm.Tests.Services
{
    public class ActividadServiceTestsSimple : ServiceTestBase
    {
        private ActividadService _actividadService;

        public ActividadServiceTestsSimple()
        {
            var unitOfWork = GetService<IUnitOfWork>();
            var logger = GetService<ILogger<ActividadService>>();
            
            _actividadService = new ActividadService(
                GetService<Microsoft.EntityFrameworkCore.IDbContextFactory<AppDbContext>>(),
                logger,
                HttpContextAccessorMock.Object,
                unitOfWork);
        }

        [Fact]
        public async Task GetSiembrasAsync_DebeRetornarListaVacia_CuandoNoHayDatos()
        {
            // Verificar que el mock esté configurado
            var httpContextAccessor = HttpContextAccessorMock.Object;
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                Console.WriteLine("Usuario autenticado correctamente en el mock");
            }
            else
            {
                Console.WriteLine("ERROR: Usuario no autenticado en el mock");
            }

            // Act
            var result = await _actividadService.GetSiembrasAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
        }
    }
}
