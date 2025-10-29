using AgroForm.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IAuthService
    {
        Task CrearPrimerUsuarioAsync();
        Task<bool> ValidateUserAsync(string email, string password);
        Task<Usuario?> GetUserByEmailAsync(string email);

    }
}
