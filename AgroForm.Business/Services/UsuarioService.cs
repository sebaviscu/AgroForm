using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using AgroForm.Model.Actividades;
using AlbaServicios.Services;
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
        public UsuarioService(IDbContextFactory<AppDbContext> contextFactory, ILogger<ServiceBase<Usuario>> logger, IHttpContextAccessor httpContextAccessor)
            : base(contextFactory, logger, httpContextAccessor)
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
            return await base.GetQuery().SingleOrDefaultAsync(x => x.Email == email);
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
    }
}
