using AgroForm.Business.Contracts;
using AgroForm.Business.Services;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Configuracion;
using AgroForm.Web.Models;
using AgroForm.Web.Models.Configuracion;
using AgroForm.Web.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Controllers
{
    [Authorize(AuthenticationSchemes = "AgroFormAuth")]
    [Route("[controller]/[action]")]
    public class ConfiguracionController : Controller
    {
        private readonly ILogger<ConfiguracionController> _logger;
        private readonly ICultivoService _cultivoService;
        private readonly IEstadoFenologicoService _estadoFenologicoService;
        private readonly ICatalogoService _catalogoService;

        public ConfiguracionController(
            ILogger<ConfiguracionController> logger,
            ICultivoService cultivoService,
            IEstadoFenologicoService estadoFenologicoService,
            ICatalogoService catalogoService)
        {
            _logger = logger;
            _cultivoService = cultivoService;
            _estadoFenologicoService = estadoFenologicoService;
            _catalogoService = catalogoService;
        }

        protected UserAuth ValidarAutorizacion()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                throw new UnauthorizedAccessException("El usuario no esta autenticado");

            var claimUser = HttpContext.User;
            var userName = UtilidadService.GetClaimValue<string>(claimUser, System.Security.Claims.ClaimTypes.Name) ?? string.Empty;

            return new UserAuth
            {
                UserName = userName,
                IdLicencia = UtilidadService.GetClaimValue<int?>(claimUser, "Licencia"),
                IdCampaña = UtilidadService.GetClaimValue<int?>(claimUser, "Campania"),
                IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, System.Security.Claims.ClaimTypes.NameIdentifier),
                IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, System.Security.Claims.ClaimTypes.Role),
                Moneda = UtilidadService.GetClaimValue<Monedas>(claimUser, "Moneda")
            };
        }

        protected IActionResult HandleException(Exception? ex, string errorMessage, string endpoint = "", object? model = null)
        {
            if (ex is UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "Access");
            }

            var gResponse = new GenericResponse<object>
            {
                Success = false,
                Message = ex == null ? errorMessage : $"{errorMessage}\n {ex.InnerException?.Message ?? ex.Message}"
            };

            _logger.LogError(ex, "Error en ConfiguracionController.{Endpoint}: {Message}", endpoint, errorMessage);

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetCultivosConfig()
        {
            try
            {
                var userAuth = ValidarAutorizacion();
                var result = await _cultivoService.GetAllForLicenseConfigAsync();

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var cultivos = result.Data ?? new List<Cultivo>();
                var configList = new List<CultivoConfigVM>();

                foreach (var cultivo in cultivos)
                {
                    var isGlobal = cultivo.IdLicencia == null;
                    bool isVisible = cultivo.Activo;

                    // Check visibility via LicenciasCultivos for global items
                    if (isGlobal && userAuth.IdLicencia.HasValue)
                    {
                        isVisible = await _cultivoService.CheckVisibilityAsync(cultivo.Id, userAuth.IdLicencia.Value);
                    }

                    configList.Add(new CultivoConfigVM
                    {
                        Id = cultivo.Id,
                        Nombre = cultivo.Nombre,
                        Descripcion = cultivo.Descripcion,
                        Orden = cultivo.Orden,
                        Activo = cultivo.Activo,
                        Color = cultivo.Color,
                        IsGlobal = isGlobal,
                        IsVisible = isVisible
                    });
                }

                var response = new GenericResponse<CultivoConfigVM>
                {
                    Success = true,
                    ListObject = configList,
                    Message = "Cultivos obtenidos correctamente"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener cultivos para configuración", "GetCultivosConfig");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCultivoVisibilidad([FromBody] ToggleVisibilidadRequest request)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (!userAuth.IdLicencia.HasValue)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No hay licencia activa" });
                }

                // Get the cultivo to check if it's global or own
                var cultivoResult = await _cultivoService.GetByIdAsync(request.Id);
                if (!cultivoResult.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Cultivo no encontrado" });
                }

                var cultivo = cultivoResult.Data;
                OperationResult opResult;

                if (cultivo.IdLicencia == null)
                {
                    // Global cultivo - use SetVisibilityAsync
                    opResult = await _cultivoService.SetVisibilityAsync(request.Id, request.Visible);
                }
                else
                {
                    // Own cultivo - use ToggleActivoAsync
                    opResult = await _cultivoService.ToggleActivoAsync(request.Id);
                }

                if (!opResult.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = opResult.ErrorMessage });
                }

                return Ok(new GenericResponse<object>
                {
                    Success = true,
                    Message = "Visibilidad actualizada correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al alternar visibilidad de cultivo", "ToggleCultivoVisibilidad", request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCultivo([FromBody] CultivoVM dto)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (!userAuth.IdLicencia.HasValue)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No hay licencia activa" });
                }

                var entity = new Cultivo
                {
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Orden = dto.Orden,
                    Color = dto.Color,
                    Activo = true,
                    IdLicencia = userAuth.IdLicencia.Value,
                    RegistrationDate = TimeHelper.GetArgentinaTime(),
                    RegistrationUser = userAuth.UserName
                };

                var result = await _cultivoService.CreateWithOrderShiftAsync(entity);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var configVM = new CultivoConfigVM
                {
                    Id = result.Data.Id,
                    Nombre = result.Data.Nombre,
                    Descripcion = result.Data.Descripcion,
                    Orden = result.Data.Orden,
                    Activo = result.Data.Activo,
                    Color = result.Data.Color,
                    IsGlobal = false,
                    IsVisible = result.Data.Activo
                };

                return Ok(new GenericResponse<CultivoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Cultivo creado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear cultivo", "CreateCultivo", dto);
            }
        }

        [HttpGet("{idCultivo}")]
        public async Task<IActionResult> GetEstadosFenologicos(int idCultivo)
        {
            try
            {
                var result = await _estadoFenologicoService.GetFenologicosByCultivoAsync(idCultivo);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var estados = result.Data ?? new List<EstadoFenologico>();
                var configList = estados.Select(e => new EstadoFenologicoConfigVM
                {
                    Id = e.Id,
                    IdCultivo = e.IdCultivo,
                    Codigo = e.Codigo,
                    Nombre = e.Nombre,
                    Descripcion = e.Descripcion,
                    Activo = e.Activo
                }).ToList();

                var response = new GenericResponse<EstadoFenologicoConfigVM>
                {
                    Success = true,
                    ListObject = configList,
                    Message = "Estados fenológicos obtenidos correctamente"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener estados fenológicos", "GetEstadosFenologicos");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleEstadoFenologico([FromBody] ToggleEstadoRequest request)
        {
            try
            {
                var result = await _estadoFenologicoService.ToggleActivoAsync(request.Id);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                return Ok(new GenericResponse<object>
                {
                    Success = true,
                    Message = "Estado fenológico actualizado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al alternar estado fenológico", "ToggleEstadoFenologico", request);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateEstadoFenologico([FromBody] EstadoFenologicoVM dto)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                var entity = new EstadoFenologico
                {
                    IdCultivo = dto.IdCultivo,
                    Codigo = dto.Codigo,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Activo = true,
                    RegistrationDate = TimeHelper.GetArgentinaTime(),
                    RegistrationUser = userAuth.UserName
                };

                var result = await _estadoFenologicoService.CreateWithValidationAsync(entity);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var configVM = new EstadoFenologicoConfigVM
                {
                    Id = result.Data.Id,
                    IdCultivo = result.Data.IdCultivo,
                    Codigo = result.Data.Codigo,
                    Nombre = result.Data.Nombre,
                    Descripcion = result.Data.Descripcion,
                    Activo = result.Data.Activo
                };

                return Ok(new GenericResponse<EstadoFenologicoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Estado fenológico creado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear estado fenológico", "CreateEstadoFenologico", dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCatalogosConfig()
        {
            try
            {
                var userAuth = ValidarAutorizacion();
                var result = await _catalogoService.GetAllForLicenseConfigAsync();

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var catalogos = result.Data ?? new List<Catalogo>();
                var configList = new List<CatalogoConfigVM>();

                foreach (var catalogo in catalogos)
                {
                    var isGlobal = catalogo.IdLicencia == null;
                    bool isVisible = catalogo.Activo;

                    // Check visibility via LicenciasCatalogos for global items
                    if (isGlobal && userAuth.IdLicencia.HasValue)
                    {
                        isVisible = await _catalogoService.CheckVisibilityAsync(catalogo.Id, userAuth.IdLicencia.Value);
                    }

                    configList.Add(new CatalogoConfigVM
                    {
                        Id = catalogo.Id,
                        Tipo = catalogo.Tipo,
                        Nombre = catalogo.Nombre,
                        Descripcion = catalogo.Descripcion,
                        Activo = catalogo.Activo,
                        IsGlobal = isGlobal,
                        IsVisible = isVisible
                    });
                }

                var response = new GenericResponse<CatalogoConfigVM>
                {
                    Success = true,
                    ListObject = configList,
                    Message = "Catálogos obtenidos correctamente"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener catálogos para configuración", "GetCatalogosConfig");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCatalogoVisibilidad([FromBody] ToggleVisibilidadRequest request)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (!userAuth.IdLicencia.HasValue)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No hay licencia activa" });
                }

                var catalogoResult = await _catalogoService.GetByIdAsync(request.Id);
                if (!catalogoResult.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Catálogo no encontrado" });
                }

                var catalogo = catalogoResult.Data;
                OperationResult opResult;

                if (catalogo.IdLicencia == null)
                {
                    opResult = await _catalogoService.SetVisibilityAsync(request.Id, request.Visible);
                }
                else
                {
                    opResult = await _catalogoService.ToggleActivoAsync(request.Id);
                }

                if (!opResult.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = opResult.ErrorMessage });
                }

                return Ok(new GenericResponse<object>
                {
                    Success = true,
                    Message = "Visibilidad actualizada correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al alternar visibilidad de catálogo", "ToggleCatalogoVisibilidad", request);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCultivo(int id)
        {
            try
            {
                var userAuth = ValidarAutorizacion();
                var result = await _cultivoService.GetByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Cultivo no encontrado" });
                }

                var cultivo = result.Data;
                var isGlobal = cultivo.IdLicencia == null;
                bool isVisible = cultivo.Activo;

                if (isGlobal && userAuth.IdLicencia.HasValue)
                {
                    isVisible = await _cultivoService.CheckVisibilityAsync(cultivo.Id, userAuth.IdLicencia.Value);
                }

                var configVM = new CultivoConfigVM
                {
                    Id = cultivo.Id,
                    Nombre = cultivo.Nombre,
                    Descripcion = cultivo.Descripcion,
                    Orden = cultivo.Orden,
                    Activo = cultivo.Activo,
                    Color = cultivo.Color,
                    IsGlobal = isGlobal,
                    IsVisible = isVisible
                };

                return Ok(new GenericResponse<CultivoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Cultivo obtenido correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener cultivo", "GetCultivo");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCultivo([FromBody] CultivoVM dto)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (dto.Id <= 0)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "ID de cultivo inválido" });
                }

                // Verify this is an owned item (cannot edit global items)
                var existingResult = await _cultivoService.GetByIdAsync(dto.Id);
                if (!existingResult.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Cultivo no encontrado" });
                }

                if (existingResult.Data.IdLicencia == null)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No se puede editar un cultivo global" });
                }

                var entity = new Cultivo
                {
                    Id = dto.Id,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Orden = dto.Orden ?? 1,
                    Color = dto.Color
                };

                var result = await _cultivoService.UpdateCultivoAsync(entity);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var configVM = new CultivoConfigVM
                {
                    Id = result.Data.Id,
                    Nombre = result.Data.Nombre,
                    Descripcion = result.Data.Descripcion,
                    Orden = result.Data.Orden,
                    Activo = result.Data.Activo,
                    Color = result.Data.Color,
                    IsGlobal = false,
                    IsVisible = result.Data.Activo
                };

                return Ok(new GenericResponse<CultivoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Cultivo actualizado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar cultivo", "UpdateCultivo", dto);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatalogo(int id)
        {
            try
            {
                var userAuth = ValidarAutorizacion();
                var result = await _catalogoService.GetByIdAsync(id);

                if (!result.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Catálogo no encontrado" });
                }

                var catalogo = result.Data;
                var isGlobal = catalogo.IdLicencia == null;
                bool isVisible = catalogo.Activo;

                if (isGlobal && userAuth.IdLicencia.HasValue)
                {
                    isVisible = await _catalogoService.CheckVisibilityAsync(catalogo.Id, userAuth.IdLicencia.Value);
                }

                var configVM = new CatalogoConfigVM
                {
                    Id = catalogo.Id,
                    Tipo = catalogo.Tipo,
                    Nombre = catalogo.Nombre,
                    Descripcion = catalogo.Descripcion,
                    Activo = catalogo.Activo,
                    IsGlobal = isGlobal,
                    IsVisible = isVisible
                };

                return Ok(new GenericResponse<CatalogoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Catálogo obtenido correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al obtener catálogo", "GetCatalogo");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCatalogo([FromBody] CatalogoVM dto)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (dto.Id <= 0)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "ID de catálogo inválido" });
                }

                // Verify this is an owned item (cannot edit global items)
                var existingResult = await _catalogoService.GetByIdAsync(dto.Id);
                if (!existingResult.Success)
                {
                    return NotFound(new GenericResponse<object> { Success = false, Message = "Catálogo no encontrado" });
                }

                if (existingResult.Data.IdLicencia == null)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No se puede editar un catálogo global" });
                }

                var entity = new Catalogo
                {
                    Id = dto.Id,
                    Tipo = dto.Tipo,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion
                };

                var result = await _catalogoService.UpdateAsync(entity);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var configVM = new CatalogoConfigVM
                {
                    Id = result.Data.Id,
                    Tipo = result.Data.Tipo,
                    Nombre = result.Data.Nombre,
                    Descripcion = result.Data.Descripcion,
                    Activo = result.Data.Activo,
                    IsGlobal = false,
                    IsVisible = result.Data.Activo
                };

                return Ok(new GenericResponse<CatalogoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Catálogo actualizado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar catálogo", "UpdateCatalogo", dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateCatalogo([FromBody] CatalogoVM dto)
        {
            try
            {
                var userAuth = ValidarAutorizacion();

                if (!userAuth.IdLicencia.HasValue)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = "No hay licencia activa" });
                }

                var entity = new Catalogo
                {
                    Tipo = dto.Tipo,
                    Nombre = dto.Nombre,
                    Descripcion = dto.Descripcion,
                    Activo = true,
                    IdLicencia = userAuth.IdLicencia.Value,
                    RegistrationDate = TimeHelper.GetArgentinaTime(),
                    RegistrationUser = userAuth.UserName
                };

                var result = await _catalogoService.CreateAsync(entity);

                if (!result.Success)
                {
                    return BadRequest(new GenericResponse<object> { Success = false, Message = result.ErrorMessage });
                }

                var configVM = new CatalogoConfigVM
                {
                    Id = result.Data.Id,
                    Tipo = result.Data.Tipo,
                    Nombre = result.Data.Nombre,
                    Descripcion = result.Data.Descripcion,
                    Activo = result.Data.Activo,
                    IsGlobal = false,
                    IsVisible = result.Data.Activo
                };

                return Ok(new GenericResponse<CatalogoConfigVM>
                {
                    Success = true,
                    Object = configVM,
                    Message = "Catálogo creado correctamente"
                });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear catálogo", "CreateCatalogo", dto);
            }
        }
    }

    #region Request DTOs

    public class ToggleVisibilidadRequest
    {
        public int Id { get; set; }
        public bool Visible { get; set; }
    }

    public class ToggleEstadoRequest
    {
        public int Id { get; set; }
    }

    #endregion
}
