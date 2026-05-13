using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business.Services
{
    public class UsuarioService : ServiceBase<Usuario>, IUsuarioService
    {
        public UsuarioService(IUnitOfWork unitOfWork, ILogger<ServiceBase<Usuario>> logger, IUserContext userContext)
            : base(unitOfWork, logger, userContext)
        {
        }

        public override async Task<OperationResult<Usuario>> CreateAsync(Usuario entity)
        {
            var validaEmail = await  base.GetQuery().SingleOrDefaultAsync(x => x.Email == entity.Email);

            if (validaEmail != null)
            {
                return OperationResult<Usuario>.Failure("El email ingresado, ya existe");
            }

            return await base.CreateAsync(entity);
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            return await base.GetQueryWithoutFilters().SingleOrDefaultAsync(x => x.Email == email);
        }

        public async Task<bool> ValidateUserAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false;

            return VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt);
        }

        private bool VerifyPasswordHash(string password, string storedHash, byte[] storedSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt);
            var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            var bytes = Convert.FromBase64String("dxaMWI4VuVyQf8+25R62xIGiPXnBcWfqOrrpXcodr7XR7mgadjxM1xkFgvhT+n+ELABiCfg4grXPWuUwjoULTmljN+YAC8Gk67hzbuS07kFtwUDxtXKeCZ79DR5Ky2PayAlXX6hh7zYwi6M/OSqVdQgj+/uUoS+s556NxtE0RwE=");
            return Convert.ToBase64String(computedHash) == storedHash;
        }

        public static void CreatePasswordHash(string password, out string passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512();
            passwordSalt = hmac.Key;
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            passwordHash = Convert.ToBase64String(hash);
        }

        public async Task<OperationResult<Usuario>> UpdateUserProfileAsync(int id, string nombre, string email, string phoneNumber)
        {
            try
            {
                // Obtener el usuario actual
                var getResult = await GetByIdAsync(id);
                if (!getResult.Success)
                {
                    return OperationResult<Usuario>.Failure("Usuario no encontrado", "NOT_FOUND");
                }

                var usuario = getResult.Data;
                
                // Verificar si el email ya está siendo usado por otro usuario
                var existingUser = await GetUserByEmailAsync(email);
                if (existingUser != null && existingUser.Id != id)
                {
                    return OperationResult<Usuario>.Failure("El email ya está siendo utilizado por otro usuario", "EMAIL_EXISTS");
                }

                // Actualizar los datos
                usuario.Nombre = nombre;
                //usuario.Email = email;
                usuario.PhoneNumber = phoneNumber;

                var updateResult = await UpdateAsync(usuario);
                
                if (updateResult.Success)
                {
                    return OperationResult<Usuario>.SuccessResult(usuario);
                }
                else
                {
                    return OperationResult<Usuario>.Failure(updateResult.ErrorMessage, updateResult.ErrorCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario con ID {Id}", id);
                return OperationResult<Usuario>.Failure($"Ocurrió un problema al actualizar el perfil: {ex.Message}", "DATABASE_ERROR");
            }
        }
    }
}
