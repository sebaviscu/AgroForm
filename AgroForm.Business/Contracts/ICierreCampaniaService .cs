using AgroForm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface ICierreCampaniaService : IServiceBase<ReporteCierreCampania>
    {
        Task<ReporteCierreCampania> GenerarReporteCierreAsync(int idCampania);
        Task<byte[]> GenerarPdfReporteAsync(int idCampania);
        Task<List<ReporteCierreCampania>> ObtenerReportesAnterioresAsync(int idLicencia);
    }
}
