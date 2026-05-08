using AgroForm.Model;
using AgroForm.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Contracts
{
    public interface IUsuarioService : IServiceBase<Usuario>
    {
        Task<bool> ValidateUserAsync(string email, string password);
        Task<Usuario?> GetUserByEmailAsync(string email);
        Task<OperationResult<Usuario>> UpdateUserProfileAsync(int id, string nombre, string email, string phoneNumber);
    }
}
