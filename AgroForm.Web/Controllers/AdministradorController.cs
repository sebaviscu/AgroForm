using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    public class AdministradorController : BaseController<Licencia, LicenciaVM, ILicenciaService>
    {
        private readonly IUsuarioService _usuarioService;

        public AdministradorController(ILogger<CampaniaController> logger, IMapper mapper, ILicenciaService service, IUsuarioService usuarioService)
            : base(logger, mapper, service)
        {
            _usuarioService = usuarioService;
        }

        public async Task<IActionResult> Index()
        {
            var userAuth = ValidarAutorizacion(new[] { Roles.Administrador });
            var userLogin = await _usuarioService.GetByIdAsync(userAuth.IdUsuario);

            if (!userLogin.Success || !userLogin.Data.SuperAdmin)
                return NotFound();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLicenciaById(int id)
        {
            var gResponse = new GenericResponse<LicenciaVM>();
            try
            {
                var licencia = await _service.GetByIdAsync(id);
                gResponse.Success = true;
                gResponse.Object = Map<Licencia, LicenciaVM>(licencia.Data);
                gResponse.Message = "Datos obtenidos correctamente";

                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear Licencia rápida");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }
        }

        [HttpPost]
        public override async Task<IActionResult> Create([FromBody] LicenciaVM dto)
        {
            var entity = Map<LicenciaVM, Licencia>(dto);
            var user = Map<UsuarioVM, Usuario>(dto.Usuario);

            var result = await _service.CreateAsync(entity);

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            user.IdLicencia = result.Data.Id;
            var resultUser = await _usuarioService.CreateAsync(user);

            if (!resultUser.Success)
            {
                gResponse.Success = false;
                gResponse.Message = resultUser.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<Licencia, LicenciaVM>(result.Data);
            gResponse.Message = "Registro creado correctamente";
            return Ok(gResponse);
        }

        [HttpPost]
        public async Task<JsonResult> CreatePagoLicencia([FromBody] PagoLicenciaVM pagoLicencia)
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                var result = _service.CreatePagarLicencia(Map<PagoLicenciaVM, PagoLicencia>(pagoLicencia));
                return Json(new { success = result, message = "Pago agregado correctamente" });
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
                var result = _service.DeletePagoLicencia(id);

                return Json(new { success = result, message = "Pago eliminado correctamente" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

}
