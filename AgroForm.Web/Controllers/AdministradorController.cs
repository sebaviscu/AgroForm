using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class AdministradorController : BaseController<Licencia, LicenciaVM, ILicenciaService>
    {
        private readonly IUsuarioService _usuarioService;

        public AdministradorController(ILogger<CampaniaController> logger, ILicenciaService service, IUsuarioService usuarioService)
            : base(logger, service)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            ValidarAutorizacion(new[] { Roles.SuperAdmin });
            return View();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLicenciaById(int id)
        {
            var gResponse = new GenericResponse<LicenciaVM>();
            try
            {
                ValidarAutorizacion(new[] { Roles.SuperAdmin });
                var licencia = await _service.GetByIdAsync(id);
                gResponse.Success = true;
                gResponse.Object = Map<Licencia, LicenciaVM>(licencia.Data);
                gResponse.Message = "Datos obtenidos correctamente";

                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Licencia con ID {Id}", id);
                gResponse.Success = false;
                gResponse.Message = "Ha ocurrido un error";
                return BadRequest(gResponse);
            }
        }

        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] LicenciaVM dto)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.SuperAdmin });
                
                var entity = Map<LicenciaVM, Licencia>(dto);
                
                // Usamos el nuevo método atómico que crea Licencia + Usuario
                var result = await _service.CreateLicenseWithAdminAsync(
                    entity, 
                    dto.Usuario.Nombre, 
                    dto.Usuario.Email, 
                    dto.Usuario.Password);

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
                _logger.LogError(ex, "Error al crear licencia");
                gResponse.Success = false;
                gResponse.Message = "Ha ocurrido un error inesperado";
                return BadRequest(gResponse);
            }
        }

        [HttpPost]
        public async Task<JsonResult> CreatePagoLicencia([FromBody] PagoLicenciaVM pagoLicencia)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = await _service.CreatePagarLicencia(Map<PagoLicenciaVM, PagoLicencia>(pagoLicencia));
                return Json(new { success = result.Success, message = result.Success ? "Pago agregado correctamente" : result.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<JsonResult> DeletePagoLicencia(int id)
        {
            try
            {
                var result = await _service.DeletePagoLicencia(id);
                return Json(new { success = result.Success, message = result.Success ? "Pago eliminado correctamente" : result.ErrorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

}
