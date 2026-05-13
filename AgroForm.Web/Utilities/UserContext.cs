using AgroForm.Business.Contracts;
using AgroForm.Model;
using AgroForm.Business.Services;
using AgroForm.Model.Configuracion;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using static AgroForm.Model.EnumClass;

namespace AgroForm.Web.Utilities
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private UserAuth _user;
        private int? _simulatedLicenciaId;
        private int? _simulatedCampaniaId;
        private bool _simulationLoaded;
        private bool _isInLoginProcess;

        private const string SimLicenciaSessionKey = "SimulacionLicenciaId";
        private const string SimCampaniaSessionKey = "SimulacionCampaniaId";

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetLoginProcess(bool isInLogin)
        {
            _isInLoginProcess = isInLogin;
        }

        public int? IdLicencia 
        {
            get 
            {
                LoadSimulationFromSessionIfNeeded();
                
                // Si ya tenemos un valor simulado, retornarlo
                if (_simulatedLicenciaId.HasValue)
                    return _simulatedLicenciaId;
                
                // Si estamos en proceso de login, no usar fallback hardcoded
                if (_isInLoginProcess)
                    return null; // Dejar que el servicio maneje sin filtro de licencia
                
                // Retornar el ID real del usuario
                return User.IdLicencia;
            }
        }

        public int? IdCampaña
        {
            get
            {
                LoadSimulationFromSessionIfNeeded();
                // Si ya tenemos un valor simulado, retornarlo
                if (_simulatedCampaniaId.HasValue)
                    return _simulatedCampaniaId;

                return User.IdCampaña;
            }
        }
        public int IdUsuario => User.IdUsuario;
        public string UserName => User.UserName;
        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
        public bool IsSuperAdmin => User.IdRol == Roles.SuperAdmin;
        
        public bool IsSimulating
        {
            get
            {
                LoadSimulationFromSessionIfNeeded();
                return IsSuperAdmin && (_simulatedLicenciaId.HasValue || _simulatedCampaniaId.HasValue);
            }
        }

        public UserAuth User
        {
            get
            {
                if (_user != null) return _user;

                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext != null && httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
                {
                    var claimUser = httpContext.User;
                    _user = new UserAuth
                    {
                        UserName = UtilidadService.GetClaimValue<string>(claimUser, ClaimTypes.Name),
                        IdLicencia = UtilidadService.GetClaimValue<int>(claimUser, "Licencia"),
                        IdCampaña = UtilidadService.GetClaimValue<int>(claimUser, "Campania"),
                        IdUsuario = UtilidadService.GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                        IdRol = UtilidadService.GetClaimValue<Roles>(claimUser, ClaimTypes.Role),
                        Moneda = UtilidadService.GetClaimValue<Monedas>(claimUser, "Moneda"),
                        IdMonedaReferencia = UtilidadService.GetClaimValue<int?>(claimUser, "IdMonedaReferencia")
                    };
                }
                else
                {
                    // Cuando no hay usuario autenticado, retornar valores nulos
                    _user = new UserAuth
                    {
                        IdLicencia = null,
                        IdCampaña = null,
                        UserName = null,
                        IdUsuario = 0
                    };
                }

                return _user;
            }
        }

        public void SetSimulatedLicencia(int? licenciaId)
        {
            if (IsSuperAdmin)
            {
                _simulatedLicenciaId = licenciaId;
                _simulationLoaded = true;
            }
        }

        public void SetSimulatedCampania(int? campaniaId)
        {
            if (IsSuperAdmin)
            {
                _simulatedCampaniaId = campaniaId;
                _simulationLoaded = true;
            }
        }

        public void ClearSimulation()
        {
            _simulatedLicenciaId = null;
            _simulatedCampaniaId = null;
            _simulationLoaded = true;
        }

        private void LoadSimulationFromSessionIfNeeded()
        {
            if (_simulationLoaded)
                return;

            var httpContext = _httpContextAccessor.HttpContext;
            var session = httpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>()?.Session;
            if (session == null)
            {
                _simulationLoaded = true;
                return;
            }

            var simulacionLicencia = session.GetString(SimLicenciaSessionKey);
            if (!string.IsNullOrWhiteSpace(simulacionLicencia) && int.TryParse(simulacionLicencia, out var licenciaId))
            {
                _simulatedLicenciaId = licenciaId;
            }

            var simulacionCampania = session.GetString(SimCampaniaSessionKey);
            if (!string.IsNullOrWhiteSpace(simulacionCampania) && int.TryParse(simulacionCampania, out var campaniaId))
            {
                _simulatedCampaniaId = campaniaId;
            }

            _simulationLoaded = true;
        }
    }
}
