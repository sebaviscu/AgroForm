using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Web.Models;
using AgroForm.Web.Models.CicloCultivo;
using AgroForm.Web.Utilities;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    public class CicloCultivoController : BaseController<CicloCultivo, CicloCultivoVM, ICicloCultivoService>
    {
        private readonly ICultivoService _cultivoService;
        private readonly ILoteService _loteService;

        public CicloCultivoController(
            ICicloCultivoService cicloService,
            ICultivoService cultivoService,
            ILoteService loteService,
            ILogger<CicloCultivoController> logger)
            : base(logger, cicloService)
        {
            _cultivoService = cultivoService;
            _loteService = loteService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                var ciclosResult = await _service.GetAllByCamapniaAsync();
                var lotesResult = await _loteService.GetAllWithDetailsAsync();
                var cultivosResult = await _cultivoService.GetAllAsync();

                var vm = new CicloCultivoIndexVM
                {
                    Ciclos = ciclosResult.Data?.Adapt<List<CicloCultivoVM>>() ?? new List<CicloCultivoVM>(),
                    Lotes = lotesResult.Data?.Select(l => new SelectListItem
                    {
                        Value = l.Id.ToString(),
                        Text = $"{l.Nombre} ({l.Campo?.Nombre ?? "Sin campo"})"
                    }).ToList() ?? new List<SelectListItem>(),
                    Cultivos = cultivosResult.Data?.Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Nombre
                    }).ToList() ?? new List<SelectListItem>()
                };

                return View(vm);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar indice de ciclos de cultivo");
                return View("Error");
            }
        }

        [HttpGet]
        public override async Task<IActionResult> GetAll()
        {
            try
            {
                var user = ValidarAutorizacion();
                var result = await _service.GetAllByCamapniaAsync();

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.ListObject = result.Data?.Adapt<List<CicloCultivoVM>>();
                gResponse.Message = "Ciclos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciclos");
                gResponse.Success = false;
                gResponse.Message = "Error al obtener ciclos";
                return BadRequest(gResponse);
            }
        }

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetById(int id)
        {
            try
            {
                var user = ValidarAutorizacion();

                if (id <= 0)
                {
                    gResponse.Success = false;
                    gResponse.Message = "Id de ciclo invalido";
                    return BadRequest(gResponse);
                }

                var result = await _service.GetByIdWithDetailsAsync(id);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Object = result.Data?.Adapt<CicloCultivoVM>();
                gResponse.Message = "Detalle del ciclo obtenido correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener detalle del ciclo");
                gResponse.Success = false;
                gResponse.Message = "Error al obtener detalle del ciclo";
                return BadRequest(gResponse);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByLote(int idLote)
        {
            try
            {
                var user = ValidarAutorizacion();

                if (idLote <= 0)
                {
                    gResponse.Success = false;
                    gResponse.Message = "Id de lote invalido";
                    return BadRequest(gResponse);
                }

                var result = await _service.ObtenerCiclosPorLoteAsync(idLote);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.ListObject = result.Data?.Adapt<List<CicloCultivoVM>>();
                gResponse.Message = "Ciclos obtenidos correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener ciclos por lote");
                gResponse.Success = false;
                gResponse.Message = "Error al obtener ciclos por lote";
                return BadRequest(gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearCicloVM model)
        {
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    gResponse.Success = false;
                    gResponse.Message = $"Error de validacion: {string.Join(", ", errors)}";
                    return BadRequest(gResponse);
                }

                var result = await _service.CrearCicloAsync(
                    model.IdLote,
                    model.IdCultivo,
                    model.Epoca);

                if (!result.Success)
                {
                    gResponse.Success = false;
                    gResponse.Message = result.ErrorMessage;
                    return BadRequest(gResponse);
                }

                gResponse.Success = true;
                gResponse.Object = result.Data?.Adapt<CicloCultivoVM>();
                gResponse.Message = "Ciclo creado correctamente";
                return Ok(gResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear ciclo");
                gResponse.Success = false;
                gResponse.Message = "Error al crear ciclo";
                return BadRequest(gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Cerrar([FromBody] CerrarCicloVM model)
        {
            var cerrarResponse = new GenericResponse<int>();
            try
            {
                var user = ValidarAutorizacion(new[] { Roles.Administrador });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    cerrarResponse.Success = false;
                    cerrarResponse.Message = $"Error de validacion: {string.Join(", ", errors)}";
                    return BadRequest(cerrarResponse);
                }

                var result = await _service.CerrarCicloAsync(model.IdCicloCultivo);

                if (!result.Success)
                {
                    cerrarResponse.Success = false;
                    cerrarResponse.Message = result.ErrorMessage;
                    return BadRequest(cerrarResponse);
                }

                cerrarResponse.Success = true;
                cerrarResponse.Message = "Ciclo cerrado correctamente";
                return Ok(cerrarResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar ciclo");
                cerrarResponse.Success = false;
                cerrarResponse.Message = "Error al cerrar ciclo";
                return BadRequest(cerrarResponse);
            }
        }
    }
}
