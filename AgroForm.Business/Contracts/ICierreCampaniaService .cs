using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface ICierreCampaniaService : IServiceBase<ReporteCierreCampania>
    {
        Task<OperationResult<ReporteCierreCampania>> GenerarReporteCierreAsync(int idCampania);
        Task<OperationResult<byte[]>> GenerarPdfReporteAsync(ReporteCierreCampania reporteCierreCampania);
        Task<OperationResult<byte[]>> GenerarPdfReporteAsync(int idCampania);
        Task<OperationResult<ReporteCierreCampania>> GetByIdCampania(int idCampania);
    }
}
