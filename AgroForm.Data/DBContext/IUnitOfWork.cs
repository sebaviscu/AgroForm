using AgroForm.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Data.DBContext
{
    public interface IUnitOfWork
    {
        //IGenericRepository<RegistroCampo> RegistrosCampo { get; }
        //IGenericRepository<Usuario> Usuarios { get; }
        Task<int> SaveAsync();
    }
}
