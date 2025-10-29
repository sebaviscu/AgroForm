using AgroForm.Data.Repository;
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
        //private IGenericRepository<RegistroCampo> _registrosCampo;
        //private IGenericRepository<Usuario> _usuarios;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        //public IGenericRepository<RegistroCampo> RegistrosCampo => _registrosCampo ??= new GenericRepository<RegistroCampo>(_context);
        //public IGenericRepository<Usuario> Usuarios => _usuarios ??= new GenericRepository<Usuario>(_context);

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
