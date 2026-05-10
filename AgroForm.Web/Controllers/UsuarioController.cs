using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class UsuarioController : BaseController<Usuario, UsuarioVM, IUsuarioService>
    {
        private readonly IMonedaService _monedaService;

        public UsuarioController(ILogger<UsuarioController> logger, IUsuarioService service, IMonedaService monedaService)
            : base(logger, service)
        {
            _monedaService = monedaService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPut]
        public async Task<IActionResult> ActualizarMonedaReferencia([FromBody] int idMoneda)
        {
            try
            {
                // Validar que la moneda exista
                var monedas = await _monedaService.GetAllAsync();
                if (!monedas.Success || monedas.Data.All(m => m.Id != idMoneda))
                {
                    var errorResponse = new GenericResponse<object>
                    {
                        Success = false,
                        Message = "La moneda especificada no existe"
                    };
                    return BadRequest(errorResponse);
                }

                // Obtener usuario actual y actualizar su configuración
                var user = ValidarAutorizacion(new[] { Roles.Administrador, Roles.SuperAdmin });
                var usuario = await _service.GetByIdAsync(user.IdUsuario);

                if (!usuario.Success)
                {
                    var errorResponse = new GenericResponse<object>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                    return NotFound(errorResponse);
                }

                usuario.Data.IdMonedaReferencia = idMoneda;
                var updateResult = await _service.UpdateAsync(usuario.Data);

                if (!updateResult.Success)
                {
                    var errorResponse = new GenericResponse<object>
                    {
                        Success = false,
                        Message = "Error al actualizar configuración: " + updateResult.ErrorMessage
                    };
                    return BadRequest(errorResponse);
                }

                // Actualizar los claims de Moneda e IdMonedaReferencia
                // para que el cambio surta efecto inmediatamente sin requerir un nuevo login
                await UpdateClaimAsync("Moneda", idMoneda.ToString());
                await UpdateClaimAsync("IdMonedaReferencia", idMoneda.ToString());
                
                // Recargar el UserContext para que tome los nuevos claims
                //_userContext.ReloadUser();

                var successResponse = new GenericResponse<object>
                {
                    Success = true,
                    Message = "Configuración de moneda actualizada correctamente"
                };
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar configuración de moneda del usuario");
                var errorResponse = new GenericResponse<object>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetConfiguracionMoneda()
        {
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador, Roles.SuperAdmin });
                var usuario = await _service.GetByIdAsync(user.IdUsuario);

                if (!usuario.Success)
                {
                    var errorResponse = new GenericResponse<ConfiguracionMonedaVM>
                    {
                        Success = false,
                        Message = "Usuario no encontrado"
                    };
                    return NotFound(errorResponse);
                }

                // Obtener todas las monedas disponibles
                var monedas = await _monedaService.GetAllAsync();

                if (!monedas.Success)
                {
                    var errorResponse = new GenericResponse<ConfiguracionMonedaVM>
                    {
                        Success = false,
                        Message = "Error al obtener las monedas disponibles"
                    };
                    return BadRequest(errorResponse);
                }

                var monedasDisponibles = monedas.Data.Adapt<List<MonedaVM>>();

                var response = new GenericResponse<ConfiguracionMonedaVM>
                {
                    Success = true,
                    Object = new ConfiguracionMonedaVM
                    {
                        IdMonedaReferencia = usuario.Data.IdMonedaReferencia ?? 0,
                        MonedasDisponibles = monedasDisponibles
                    },
                    Message = "Configuración obtenida correctamente"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de moneda del usuario");
                var errorResponse = new GenericResponse<ConfiguracionMonedaVM>
                {
                    Success = false,
                    Message = "Error interno del servidor"
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}
