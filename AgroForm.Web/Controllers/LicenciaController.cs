using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class LicenciaController : BaseController<Licencia, LicenciaVM, ILicenciaService>
    {
        public LicenciaController(ILogger<LicenciaController> logger, ILicenciaService service)
            : base(logger, service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Obtiene los datos de la licencia del usuario logueado actual, incluyendo pagos.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyLicencia()
        {
            try
            {
                // Obtener IdLicencia del usuario autenticado
                var claimUser = HttpContext.User;
                var idLicencia = UtilidadService.GetClaimValue<int?>(claimUser, "Licencia");

                if (!idLicencia.HasValue)
                {
                    return Ok(new GenericResponse<LicenciaVM>
                    {
                        Success = false,
                        Message = "No se encontró una licencia asociada a su usuario"
                    });
                }

                var result = await _service.GetByIdAsync(idLicencia.Value);
                if (!result.Success)
                {
                    return Ok(new GenericResponse<LicenciaVM>
                    {
                        Success = false,
                        Message = result.ErrorMessage
                    });
                }

                var licenciaVM = Map<Licencia, LicenciaVM>(result.Data);

                return Ok(new GenericResponse<LicenciaVM>
                {
                    Success = true,
                    Object = licenciaVM,
                    Message = "Datos de licencia obtenidos correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener datos de licencia", "GetMyLicencia");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWithAdmin([FromBody] LicenciaConUsuarioVM request)
        {
            try
            {
                ValidarAutorizacion(new[] { Model.EnumClass.Roles.SuperAdmin });
                
                // Validaciones básicas
                if (request.Usuario == null)
                {
                    gResponse.Success = false;
                    gResponse.Message = "Los datos del usuario administrador son obligatorios";
                    return BadRequest(gResponse);
                }

                if (string.IsNullOrEmpty(request.Usuario.Nombre) || 
                    string.IsNullOrEmpty(request.Usuario.Email) || 
                    string.IsNullOrEmpty(request.Usuario.Password))
                {
                    gResponse.Success = false;
                    gResponse.Message = "Nombre, email y contraseña del administrador son obligatorios";
                    return BadRequest(gResponse);
                }

                // Validar que las contraseñas coincidan
                if (request.Usuario.Password != request.Usuario.RepetirPassword)
                {
                    gResponse.Success = false;
                    gResponse.Message = "Las contraseñas no coinciden";
                    return BadRequest(gResponse);
                }

                // Mapear licencia
                var licencia = new Licencia
                {
                    RazonSocial = request.RazonSocial,
                    NombreContacto = request.NombreContacto,
                    NumeroContacto = request.NumeroContacto,
                    TipoLicencia = request.TipoLicencia,
                    EsPrueba = request.EsPrueba,
                    FechaFinPrueba = request.FechaFinPrueba,
                    Activo = true
                };

                // Usar el método existente del servicio
                var result = await _service.CreateLicenseWithAdminAsync(licencia, request.Usuario.Nombre, request.Usuario.Email, request.Usuario.Password);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Object = Map<Licencia, LicenciaVM>(result.Data);
                gResponse.Message = "Licencia y usuario administrador creados correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear licencia con administrador", "CreateWithAdmin", request);
            }
        }

        
        [HttpPost]
        public async Task<IActionResult> CreatePagoLicencia([FromBody] PagoLicenciaVM pago)
        {
            try
            {
                ValidarAutorizacion(new[] { Model.EnumClass.Roles.SuperAdmin });
                
                var pagoEntity = Map<PagoLicenciaVM, PagoLicencia>(pago);
                var result = await _service.CreatePagarLicencia(pagoEntity);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Message = "Pago agregado correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al agregar pago de licencia", "CreatePagoLicencia", pago);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeletePagoLicencia(int idPago)
        {
            try
            {
                ValidarAutorizacion(new[] { Model.EnumClass.Roles.SuperAdmin });
                
                var result = await _service.DeletePagoLicencia(idPago);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Message = "Pago eliminado correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar pago de licencia", "DeletePagoLicencia", idPago);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByIdWithPagos(int id)
        {
            try
            {
                ValidarAutorizacion(new[] { Model.EnumClass.Roles.SuperAdmin });
                
                var result = await _service.GetByIdAsync(id);
                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return NotFound(gResponse);
                }

                // Usar mapper para incluir los pagos
                var licenciaVM = Map<Licencia, LicenciaVM>(result.Data);

                gResponse.Success = true;
                gResponse.Object = licenciaVM;
                gResponse.Message = "Licencia encontrada con pagos";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener licencia con pagos", "GetByIdWithPagos", id);
            }
        }
    }
}
