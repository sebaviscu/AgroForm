using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Services
{
    public class UtilidadService
    {
        public static T GetClaimValue<T>(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim == null)
                return default;

            try
            {
                if (typeof(T).IsEnum)
                {
                    if (Enum.TryParse(typeof(T), claim.Value, out var result))
                    {
                        return (T)result;
                    }
                    return default;
                }

                return (T)Convert.ChangeType(claim.Value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        public static decimal? CalcularCostoARS(decimal? costo, bool esDolar, decimal tipoCambioUSD)
        {
            if (costo == null) return null;
            return esDolar ? costo * tipoCambioUSD : costo;
        }

        public static decimal? CalcularCostoUSD(decimal? costo, bool esDolar, decimal tipoCambioUSD)
        {
            if (costo == null) return null;
            return esDolar ? costo : costo / tipoCambioUSD;
        }
    }
}
