using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Data.Repository
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filtro);
        //Task<TEntity?> GetAsNoTrackingAsync(Expression<Func<TEntity, bool>> filtro);
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filtro = null);
        Task<TEntity> AddAsync(TEntity entidad);
        Task AddRangeAsync(IEnumerable<TEntity> entidades);
        Task<bool> UpdateAsync(TEntity entidad);
        Task<bool> UpdateRangeAsync(IEnumerable<TEntity> entidades);
        Task<bool> DeleteAsync(TEntity entidad);
        Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entidades);
        IQueryable<TEntity> Query();

    }
}
