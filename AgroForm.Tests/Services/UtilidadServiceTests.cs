using System.Security.Claims;
using AgroForm.Business.Services;
using Xunit;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Tests.Services
{
    public class UtilidadServiceTests
    {
        [Fact]
        public void GetClaimValue_DebeRetornarValorString_CuandoClaimExiste()
        {
            // Arrange
            var claims = new[] { new Claim("test-claim", "test-value") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<string>(user, "test-claim");

            // Assert
            Assert.Equal("test-value", result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarDefault_CuandoClaimNoExiste()
        {
            // Arrange
            var claims = new[] { new Claim("other-claim", "value") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<string>(user, "non-existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarInt_CuandoClaimEsNumerico()
        {
            // Arrange
            var claims = new[] { new Claim("id", "42") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<int>(user, "id");

            // Assert
            Assert.Equal(42, result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarDefault_CuandoClaimNoEsNumerico()
        {
            // Arrange
            var claims = new[] { new Claim("id", "not-a-number") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<int>(user, "id");

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarEnum_CuandoClaimEsValorValido()
        {
            // Arrange
            var claims = new[] { new Claim("rol", "Administrador") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<Roles>(user, "rol");

            // Assert
            Assert.Equal(Roles.Administrador, result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarDefault_CuandoClaimNoEsEnumValido()
        {
            // Arrange
            var claims = new[] { new Claim("rol", "InvalidRole") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<Roles>(user, "rol");

            // Assert
            Assert.Equal(default(Roles), result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarNullableInt_CuandoClaimExiste()
        {
            // Arrange
            var claims = new[] { new Claim("nullable-id", "99") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<int?>(user, "nullable-id");

            // Assert
            Assert.Equal(99, result);
        }

        [Fact]
        public void GetClaimValue_DebeRetornarNull_CuandoClaimNoExisteParaNullable()
        {
            // Arrange
            var claims = Array.Empty<Claim>();
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var user = new ClaimsPrincipal(identity);

            // Act
            var result = UtilidadService.GetClaimValue<int?>(user, "non-existent");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CalcularCostoARS_DebeRetornarCostoDirecto_CuandoNoEsDolar()
        {
            // Act
            var result = UtilidadService.CalcularCostoARS(100m, false, 1000m);

            // Assert
            Assert.Equal(100m, result);
        }

        [Fact]
        public void CalcularCostoARS_DebeMultiplicarPorTipoCambio_CuandoEsDolar()
        {
            // Act
            var result = UtilidadService.CalcularCostoARS(100m, true, 1200m);

            // Assert
            Assert.Equal(120000m, result);
        }

        [Fact]
        public void CalcularCostoARS_DebeRetornarNull_CuandoCostoEsNull()
        {
            // Act
            var result = UtilidadService.CalcularCostoARS(null, false, 1000m);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void CalcularCostoUSD_DebeRetornarCostoDirecto_CuandoEsDolar()
        {
            // Act
            var result = UtilidadService.CalcularCostoUSD(100m, true, 1000m);

            // Assert
            Assert.Equal(100m, result);
        }

        [Fact]
        public void CalcularCostoUSD_DebeDividirPorTipoCambio_CuandoNoEsDolar()
        {
            // Act
            var result = UtilidadService.CalcularCostoUSD(120000m, false, 1200m);

            // Assert
            Assert.Equal(100m, result);
        }

        [Fact]
        public void CalcularCostoUSD_DebeRetornarNull_CuandoCostoEsNull()
        {
            // Act
            var result = UtilidadService.CalcularCostoUSD(null, false, 1000m);

            // Assert
            Assert.Null(result);
        }
    }
}
