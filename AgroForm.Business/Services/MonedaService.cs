using AgroForm.Business.Contracts;
using AgroForm.Business.Externos.DolarApi;
using AgroForm.Data.DBContext;
using AgroForm.Model;
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
    public class MonedaService : ServiceBase<Moneda>, IMonedaService
    {
        public MonedaService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Moneda>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
        {
        }

        public async Task<Moneda> ObtenerTipoCambioActualAsync()
        {
            return await GetQuery().FirstAsync(m => m.Id == (int)_userAuth.Moneda);
        }

        public async Task<bool> ActualizarMonedasCotizacionAsync(List<DolarInfo> dolarInfos)
        {
            try
            {
                var monedas = (await base.GetAllAsync()).Data;

                foreach (var item in dolarInfos)
                {
                    var cotizacion = monedas.FirstOrDefault(_ => _.Nombre.ToUpper() == item.Nombre.ToUpper());
                    var precio = (item.Venta + item.Compra) / 2;

                    if (cotizacion == null)
                    {
                        var newMoneda = new Moneda()
                        {
                            Nombre = item.Nombre,
                            Codigo = "USD",
                            Simbolo = "US$",
                            TipoCambioReferencia = precio,
                            ModificationDate = item.FechaActualizacion,
                            ModificationUser = "DolarApi"
                        };
                        var result = await base.CreateAsync(newMoneda);
                        if(!result.Success)
                            throw new Exception("Error al crear nueva moneda: " + result.ErrorMessage);
                    }
                    else
                    {
                        cotizacion.TipoCambioReferencia = precio;
                        cotizacion.ModificationDate = item.FechaActualizacion;
                        cotizacion.ModificationUser = "DolarApi";
                        await base.UpdateAsync(cotizacion);
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                base._logger.LogError("Error al actualizar cotizaciones", e);
                return false;
            }

        }
    }
}
