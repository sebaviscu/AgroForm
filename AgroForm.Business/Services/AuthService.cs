using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AgroForm.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AgroForm.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IDbContextFactory<AppDbContext> _contextFactory;

        public AuthService(IDbContextFactory<AppDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task CrearPrimerUsuarioAsync()
        {
            using var context = _contextFactory.CreateDbContext();

            CreatePasswordHash("123456", out string passwordHash, out byte[] passwordSalt);

            var usuario = new Usuario
            {
                IdLicencia = 1,
                Nombre = "Administrador",
                Email = "admin@agroform.com",
                Rol = EnumClass.Roles.Administrador,
                Activo = true,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                EmailConfirmed = true,
                RegistrationDate = DateTime.Now,
                RegistrationUser = "System"
            };

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
        }

        public async Task<Usuario?> GetUserByEmailAsync(string email)
        {
            using var context = _contextFactory.CreateDbContext();
            return await context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.Activo);
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
            Console.WriteLine("0x" + BitConverter.ToString(bytes).Replace("-", ""));
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
