using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IChartGeneratorService
    {
        Task<byte[]> GenerarGraficoTortaAsync(Dictionary<string, decimal> datos, string titulo);
        Task<byte[]> GenerarGraficoBarrasAsync(Dictionary<string, decimal> datos, string titulo);
    }
}
