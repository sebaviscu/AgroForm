using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Externos.DolarApi
{
    public interface IDolarApiService
    {
        Task<OperationResult<List<DolarInfo>>> ObtenerCotizacionesAsync();
    }
}
