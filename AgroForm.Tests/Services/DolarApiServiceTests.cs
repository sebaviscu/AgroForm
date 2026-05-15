using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using AgroForm.Business.Externos.DolarApi;
using AgroForm.Business.Services;
using AgroForm.Model;
using Xunit;

namespace AgroForm.Tests.Services
{
    public class DolarApiServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<DolarApiService>> _loggerMock;

        public DolarApiServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://dolarapi.com/")
            };
            _loggerMock = new Mock<ILogger<DolarApiService>>();
        }

        [Fact]
        public async Task ObtenerCotizacionesAsync_DebeRetornarListaDeCotizaciones_CuandoApiRespondeOk()
        {
            // Arrange
            var dolaresEsperados = new List<DolarInfo>
            {
                new DolarInfo
                {
                    Moneda = "USD",
                    Casa = "oficial",
                    Nombre = "Dólar Oficial",
                    Compra = 1000.50m,
                    Venta = 1020.75m,
                    FechaActualizacion = TimeHelper.GetArgentinaTime()
                },
                new DolarInfo
                {
                    Moneda = "USD",
                    Casa = "blue",
                    Nombre = "Dólar Blue",
                    Compra = 1200.00m,
                    Venta = 1220.00m,
                    FechaActualizacion = TimeHelper.GetArgentinaTime()
                }
            };

            var jsonResponse = JsonSerializer.Serialize(dolaresEsperados, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json")
                });

            var service = new DolarApiService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.ObtenerCotizacionesAsync();

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Equal("oficial", result.Data[0].Casa);
            Assert.Equal("blue", result.Data[1].Casa);
            Assert.Equal(1000.50m, result.Data[0].Compra);
            Assert.Equal(1220.00m, result.Data[1].Venta);
        }

        [Fact]
        public async Task ObtenerCotizacionesAsync_DebeRetornarFallo_CuandoApiRespondeError()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError
                });

            var service = new DolarApiService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.ObtenerCotizacionesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("HTTP_CLIENT_FAILED", result.ErrorCode);
        }

        [Fact]
        public async Task ObtenerCotizacionesAsync_DebeRetornarFallo_CuandoHttpClientLanzaExcepcion()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var service = new DolarApiService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.ObtenerCotizacionesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("HTTP_CLIENT_FAILED", result.ErrorCode);
        }

        [Fact]
        public async Task ObtenerCotizacionesAsync_DebeRetornarFallo_CuandoJsonEsInvalido()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("invalid json", System.Text.Encoding.UTF8, "application/json")
                });

            var service = new DolarApiService(_httpClient, _loggerMock.Object);

            // Act
            var result = await service.ObtenerCotizacionesAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Equal("HTTP_CLIENT_FAILED", result.ErrorCode);
        }
    }
}
