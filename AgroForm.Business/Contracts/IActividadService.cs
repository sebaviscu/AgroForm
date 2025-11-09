using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Business.Contracts
{
    public interface IActividadService
    {
        Task<OperationResult<List<LaborDTO>>> GetLaboresByAsync(int? IdCampania = null, int? IdLote = null, List<int> IdsLotes = null);

        Task SaveActividadAsync(List<ILabor> actividades);
        Task DeteleActividadAsync(int idActividad, TipoActividadEnum IdTipoActividad);

        //Task<List<Actividad>> GetByidCampoAsync(List<int> lotesId);

        //Task<OperationResult<List<Actividad>>> GetRecentAsync();

    }
}
