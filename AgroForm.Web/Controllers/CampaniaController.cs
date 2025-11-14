using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Web.Models;
using AgroForm.Web.Models.IndexVM;
using AgroForm.Web.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CampaniaController : BaseController<Campania, CampaniaVM, ICampaniaService>
    {
        private readonly ILoteService _loteService;

        public CampaniaController(ILogger<CampaniaController> logger, IMapper mapper, ICampaniaService service, ILoteService loteService)
            : base(logger, mapper, service)
        {
            _loteService = loteService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                ValidarAutorizacion(new[] { Roles.Administrador });

                //var campanias = await _service.GetAllWithDetailsAsync();

                //if (!campanias.Success)
                //{
                //    return BadRequest(campanias.ErrorMessage);
                //}

                //var vm = new CampaniasIndexVM
                //{
                //    Campanias = Map<List<Campania>, List<CampaniaVM>>(campanias.Data),
                //    Estados = new List<SelectListItem>
                //{
                //    new SelectListItem { Value = "EnCurso", Text = "En Curso" },
                //    new SelectListItem { Value = "Finalizada", Text = "Finalizada" },
                //    new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
                //}
                //};

                //return View(vm);
                return View();
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
            catch (Exception ex)
            {
                return HandleException(ex,"Error al iniciar la pagina", "Index");
            }
        }


        [HttpGet]
        public override async Task<IActionResult> GetAllDataTable()
        {
            var result = await _service.GetAllWithDetailsAsync();
            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            var dataVM = Map<List<Campania>, List<CampaniaVM>>(result.Data);

            return Json(new
            {
                success = true,
                data = dataVM
            });
        }

        [HttpPut]
        public override async Task<IActionResult> Update([FromBody] CampaniaVM dto)
        {
            var entity = Map<CampaniaVM, Campania>(dto);
            var result = await _service.UpdateAsync(entity);

            await _loteService.CreateRangeAsync(entity.Lotes.ToList());

            if (!result.Success)
            {
                gResponse.Success = false;
                gResponse.Message = result.ErrorMessage;
                return BadRequest(gResponse);
            }

            gResponse.Success = true;
            gResponse.Object = Map<Campania, CampaniaVM>(result.Data);
            gResponse.Message = "Registro actualizado correctamente";
            return Ok(gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> Finalizar(int id)
        {
            var gResponse = new GenericResponse<int>();

            try
            {

                gResponse.Success = true;
                gResponse.Message = "Campaña finalizada";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al borrar labor");
                gResponse.Success = false;
                gResponse.Message = "Ah ocurrido un error";
                return BadRequest(gResponse);
            }

        }
    }
}
