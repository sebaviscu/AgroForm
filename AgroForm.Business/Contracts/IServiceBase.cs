using AgroForm.Model;
using AlbaServicios.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IServiceBase<T> where T : EntityBase
    {
        Task<OperationResult<List<T>>> GetAllAsync();
        Task<OperationResult<List<T>>> GetAllByCamapniaAsync();
        Task<OperationResult<List<T>>> GetAllWithDetailsAsync();
        Task<OperationResult<T>> GetByIdAsync(long id);
        Task<OperationResult<T>> GetByIdWithDetailsAsync(long id);
        Task<OperationResult<T>> CreateAsync(T entity);
        Task<OperationResult<List<T>>> CreateRangeAsync(List<T> entityList);
        Task<OperationResult<T>> UpdateAsync(T entity);
        Task<OperationResult<List<T>>> UpdateRangeAsync(List<T> entityList);
        Task<OperationResult> DeleteAsync(long id);
        Task<OperationResult> DeleteRangeAsync(List<long> ids);
        Task<bool> ExistsAsync(long id);
        Task<OperationResult> ValidateAsync(T entity);
        IQueryable<T> GetQuery();
    }
}
