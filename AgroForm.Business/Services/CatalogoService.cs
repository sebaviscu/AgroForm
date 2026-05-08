using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Services
{
    public class CatalogoService : ServiceBase<Catalogo>, ICatalogoService
    {
        public CatalogoService(IUnitOfWork unitOfWork, ILogger<CatalogoService> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public async Task<OperationResult<List<Catalogo>>> GetByType(TipoCatalogoEnum tipo)
        {
            try
            {
                var entities = await _repository.GetAllAsync(e => e.Tipo == tipo && e.Activo);

                if (!entities.Any())
                    return OperationResult<List<Catalogo>>.Failure("Catálogo por tipo no encontrado");

                return OperationResult<List<Catalogo>>.SuccessResult(entities.OrderBy(e => e.Nombre).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Catálogos por tipo {tipo}", tipo);
                return OperationResult<List<Catalogo>>.Failure("Error al obtener Catálogos por tipo");
            }
        }

        public async Task<OperationResult<List<Catalogo>>> GetAllActive()
        {
            try
            {
                var entities = await _repository.GetAllAsync(e => e.Activo);

                if (!entities.Any())
                    return OperationResult<List<Catalogo>>.Failure("Catálogos activos no encontrados");

                return OperationResult<List<Catalogo>>.SuccessResult(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los Catálogos activos");
                return OperationResult<List<Catalogo>>.Failure("Error al obtener todos los Catálogos activos");
            }
        }
    }
}
