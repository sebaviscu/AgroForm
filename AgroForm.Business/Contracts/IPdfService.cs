using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
  public interface IPdfService
  {
      Task<OperationResult<byte[]>> GenerarPdfCierreCampaniaAsync(ReporteCierreCampania reporte);
  }
  
}
