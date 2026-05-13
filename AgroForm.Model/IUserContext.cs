using AgroForm.Model.Configuracion;

namespace AgroForm.Model
{
    public interface IUserContext
    {
        int? IdLicencia { get; }
        int? IdCampaña { get; }
        int IdUsuario { get; }
        string UserName { get; }
        bool IsAuthenticated { get; }
        bool IsSuperAdmin { get; }
        bool IsSimulating { get; }
        UserAuth User { get; }
        
        void SetSimulatedLicencia(int? licenciaId);
        void SetSimulatedCampania(int? campaniaId);
        void ClearSimulation();
        void SetLoginProcess(bool isInLogin);
    }
}
