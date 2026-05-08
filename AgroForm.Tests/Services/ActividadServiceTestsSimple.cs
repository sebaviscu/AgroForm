using AgroForm.Business.Services;
using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using Microsoft.Extensions.Logging;

namespace AgroForm.Tests.Services
{
    public class ActividadServiceTestsSimple : ServiceTestBase
    {
        private ActividadService _actividadService;

        public ActividadServiceTestsSimple()
        {
            _actividadService = (ActividadService)GetService<IActividadService>();
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
