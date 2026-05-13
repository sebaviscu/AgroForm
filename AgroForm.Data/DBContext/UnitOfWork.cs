using AgroForm.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Data.DBContext
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly Dictionary<Type, object> _repositories = new();

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public Microsoft.EntityFrameworkCore.DbContext Context => _context;

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IGenericRepository<T> Repository<T>() where T : class
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_context);
                _repositories[type] = repositoryInstance;
            }

            return (IGenericRepository<T>)_repositories[type];
        }
    }
}
