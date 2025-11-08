using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Configuracion;
using AlbaServicios.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Services
{
    internal class CampaniaService : ServiceBase<Campania>, ICampaniaService
    {
        public CampaniaService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Campania>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public override async Task<OperationResult<Campania>> CreateAsync(Campania entity)
        {
            try
            {
                await using var context = await _contextFactory.CreateDbContextAsync();

                var validationResult = await ValidateAsync(entity);
                if (!validationResult.Success)
                    return OperationResult<Campania>.Failure(validationResult.ErrorMessage);

                var hasCampaniaEnCurso = await GetCurrent();

                if(hasCampaniaEnCurso.Success && hasCampaniaEnCurso.Data != null)
                {
                    return OperationResult<Campania>.Failure("Existe una campaña en curso.", "SAVE_FAILED");
                }

                foreach (var item in entity.Lotes)
                {
                    item.IdLicencia = _userAuth.IdLicencia;
                    entity.RegistrationUser = _userAuth.UserName;
                    entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                }

                entity.IdLicencia = _userAuth.IdLicencia;
                entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                entity.ModificationDate = null;
                entity.RegistrationUser = _userAuth.UserName;

                context.Campanias.Add(entity);
                int result = await context.SaveChangesAsync();

                if (result > 0)
                    return OperationResult<Campania>.SuccessResult(entity);

                return OperationResult<Campania>.Failure("No se pudo insertar el registro en la base de datos.", "SAVE_FAILED");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al insertar el registro Campania");
                return OperationResult<Campania>.Failure($"Ocurrió un problema al insertar el registro: {ex.Message}", "DATABASE_ERROR");
            }
        }

        public async Task<OperationResult<Campania>> GetCurrent()
        {
            try
            {
                var campania = await GetQuery().SingleOrDefaultAsync(_ => _.IdLicencia == _userAuth.IdLicencia &&
                                                                _.EstadosCampania == EnumClass.EstadosCamapaña.EnCurso);

                if (campania == null)
                {
                    return OperationResult<Campania>.Failure($"No existe una Campaña en curso", "NOT_FOUND");
                }

                return OperationResult<Campania>.SuccessResult(campania);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al reuperar campaña en cusro Campania");
                return OperationResult<Campania>.Failure($"Ocurrió un problema al  reuperar campaña en cusro Campania: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
