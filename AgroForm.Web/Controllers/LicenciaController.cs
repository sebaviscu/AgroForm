using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
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

        [HttpPost]
        public async Task<IActionResult> CreateWithAdmin([FromBody] dynamic request)
        {
            try
            {
                ValidarAutorizacion(new[] { Model.EnumClass.Roles.SuperAdmin });
                
                // Extraer datos de la licencia
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

                // Extraer datos del usuario
                var adminName = request.Usuario?.Nombre?.ToString();
                var adminEmail = request.Usuario?.Email?.ToString();
                var adminPassword = request.Usuario?.Password?.ToString();

                if (string.IsNullOrEmpty(adminName) || string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
                {
                    gResponse.Success = false;
                    gResponse.Message = "Los datos del usuario administrador son obligatorios";
                    return BadRequest(gResponse);
                }

                // Usar el método especializado para crear licencia con admin
                var result = await _service.CreateLicenseWithAdminAsync(licencia, adminName, adminEmail, adminPassword);

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
