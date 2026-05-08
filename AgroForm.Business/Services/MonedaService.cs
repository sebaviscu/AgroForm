using AgroForm.Business.Contracts;
using AgroForm.Business.Externos.DolarApi;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Business.Services
{
    public class MonedaService : ServiceBase<Moneda>, IMonedaService
    {
        public MonedaService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Moneda>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public async Task<Moneda> ObtenerTipoCambioActualAsync()
        {
            return await GetQuery().FirstAsync(m => m.Id == (int)_userContext.User.Moneda);
        }

        public async Task<bool> ActualizarMonedasCotizacionAsync(List<DolarInfo> dolarInfos)
        {
            try
            {
                var monedas = (await base.GetAllAsync()).Data;

                foreach (var item in dolarInfos)
                {
                    var cotizacion = monedas.FirstOrDefault(_ => _.Nombre.ToUpper() == item.Nombre.ToUpper());
                    //var precio = (item.Venta + item.Compra) / 2;

                    if (cotizacion == null)
                    {
                        var newMoneda = new Moneda()
                        {
                            Nombre = item.Nombre,
                            Codigo = "USD",
                            Simbolo = "US$",
                            //TipoCambioReferencia = precio,
                            TipoCambioReferencia = item.Venta,
                            ModificationDate = item.FechaActualizacion,
                            ModificationUser = "DolarApi"
                        };
                        var result = await base.CreateAsync(newMoneda);
                        if(!result.Success)
                            throw new Exception("Error al crear nueva moneda: " + result.ErrorMessage);
                    }
                    else
                    {
                        //cotizacion.TipoCambioReferencia = precio;
                        cotizacion.TipoCambioReferencia = item.Venta;
                        cotizacion.ModificationDate = item.FechaActualizacion;
                        cotizacion.ModificationUser = "DolarApi";
                        await base.UpdateAsync(cotizacion);
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                base._logger.LogError(e, "Error al actualizar las monedas desde DolarApi");
                return false;
            }

        }
    }
}
