using AgroForm.Model.Unidades;

namespace AgroForm.Business.Contracts
{
    public interface IUnidadMedidaService
    {
        /// <summary>
        /// Obtiene una unidad de medida por su ID.
        /// </summary>
        Task<UnidadMedida?> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene la configuración de campos con sus unidades permitidas para un tipo de actividad.
        /// </summary>
        Task<List<CampoUnidadConfigDto>> GetConfigUnidadesAsync(int idTipoActividad);

        /// <summary>
        /// Valida que una unidad de medida esté permitida para un campo-labor específico.
        /// </summary>
        Task<bool> ValidarUnidadPermitidaAsync(int idCampoLaborUnidad, int idUnidadMedida);
    }
}
