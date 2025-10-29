namespace AgroForm.Data.Repository
{
    using AgroForm.Data.DBContext;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Storage;
    using System.Linq.Expressions;

    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public GenericRepository(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }

        public async Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> filtro)
        {
            await using var context = _contextFactory.CreateDbContext();
            return await context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(filtro);
        }

        public async Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filtro = null)
        {
            await using var context = _contextFactory.CreateDbContext();
            IQueryable<TEntity> query = context.Set<TEntity>();
            if (filtro != null)
                query = query.Where(filtro);
            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<TEntity> AddAsync(TEntity entidad)
        {
            await using var context = _contextFactory.CreateDbContext();
            await context.Set<TEntity>().AddAsync(entidad);
            await context.SaveChangesAsync();
            return entidad;
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entidades)
        {
            await using var context = _contextFactory.CreateDbContext();
            await context.Set<TEntity>().AddRangeAsync(entidades);
            await context.SaveChangesAsync();
        }

        public async Task<bool> UpdateRangeAsync(IEnumerable<TEntity> entidades)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.UpdateRange(entidades);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteRangeAsync(IEnumerable<TEntity> entidades)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.RemoveRange(entidades);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(TEntity entidad)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Update(entidad);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(TEntity entidad)
        {
            await using var context = _contextFactory.CreateDbContext();
            context.Remove(entidad);
            await context.SaveChangesAsync();
            return true;
        }
    }

}
