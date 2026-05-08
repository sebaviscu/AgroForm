namespace AgroForm.Data.Repository
{
    using AgroForm.Data.DBContext;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Linq.Expressions;

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext _context;

        public GenericRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filtro)
        {
            return await _context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filtro);
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filtro = null)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            if (filtro != null)
                query = query.Where(filtro);
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<TEntity> AddAsync(TEntity entidad)
        {
            await _context.Set<TEntity>().AddAsync(entidad);
            return entidad;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entidades)
        {
            await _context.Set<TEntity>().AddRangeAsync(entidades);
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<TEntity> entidades)
        {
            _context.UpdateRange(entidades);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entidades)
        {
            _context.RemoveRange(entidades);
            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateAsync(TEntity entidad)
        {
            _context.Update(entidad);
            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteAsync(TEntity entidad)
        {
            _context.Remove(entidad);
            return await Task.FromResult(true);
        }
        public async Task<TEntity?> GetByIdAsync(object id)
        {
            return await _context.Set<TEntity>().FindAsync(id);
        }

        public async Task<bool> DeleteByIdAsync(object id)
        {
            var entidad = await _context.Set<TEntity>().FindAsync(id);
            if (entidad == null)
                return false;

            _context.Remove(entidad);
            return true;
        }

        public IQueryable<TEntity> Query()
        {
            return _context.Set<TEntity>().AsQueryable();
        }

    }
}
