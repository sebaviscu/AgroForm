using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model.Unidades;
using Microsoft.EntityFrameworkCore;

namespace AgroForm.Business.Services
{
    public class UnidadMedidaService : IUnidadMedidaService
    {
        private readonly AppDbContext _context;

        public UnidadMedidaService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UnidadMedida?> GetByIdAsync(int id)
        {
            return await _context.UnidadesMedida
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<List<CampoUnidadConfigDto>> GetConfigUnidadesAsync(int idTipoActividad)
        {
            var config = await _context.CamposLaborUnidad
                .AsNoTracking()
                .Where(c => c.IdTipoActividad == idTipoActividad && c.Activo)
                .OrderBy(c => c.Orden)
                .ThenBy(c => c.NombreCampo)
                .Select(c => new CampoUnidadConfigDto
                {
                    NombreCampo = c.NombreCampo,
                    NombrePropiedad = c.NombrePropiedad,
                    Etiqueta = c.Etiqueta,
                    Requerido = c.Requerido,
                    IdUnidadPredeterminada = c.UnidadesPermitidas
                        .Where(up => up.EsPredeterminado)
                        .Select(up => (int?)up.IdUnidadMedida)
                        .FirstOrDefault(),
                    Unidades = c.UnidadesPermitidas
                        .OrderBy(up => up.Orden)
                        .Select(up => new UnidadPermitidaDto
                        {
                            Id = up.UnidadMedida.Id,
                            Nombre = up.UnidadMedida.Nombre,
                            Sigla = up.UnidadMedida.Sigla,
                            Categoria = up.UnidadMedida.Categoria.ToString(),
                            FactorConversion = up.UnidadMedida.FactorConversion,
                            EsPredeterminado = up.EsPredeterminado
                        })
                        .ToList()
                })
                .ToListAsync();

            return config;
        }

        public async Task<bool> ValidarUnidadPermitidaAsync(int idCampoLaborUnidad, int idUnidadMedida)
        {
            return await _context.CamposLaborUnidadPermitida
                .AsNoTracking()
                .AnyAsync(up =>
                    up.IdCampoLaborUnidad == idCampoLaborUnidad &&
                    up.IdUnidadMedida == idUnidadMedida);
        }
    }
}
