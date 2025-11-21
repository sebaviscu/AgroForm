using AgroForm.Business.Externos.DolarApi;
using AgroForm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IMonedaService : IServiceBase<Moneda>
    {
        Task<Moneda> ObtenerTipoCambioActualAsync();
        Task<bool> ActualizarMonedasCotizacionAsync(List<DolarInfo> dolarInfos);
    }
}
