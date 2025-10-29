using AgroForm.Business.Contracts;
using AgroForm.Data.DBContext;
using AlbaServicios.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Business
{
    public static class StartupHelper
    {
        public static void ConfigurarServicios(IServiceCollection services)
        {
            services.AddScoped(typeof(IServiceBase<>), typeof(ServiceBase<>));

            var assembly = Assembly.Load("AgroForm.Business");

            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
                .ToList();

            foreach (var implementationType in types)
            {
                var interfaceType = implementationType.GetInterfaces()
                    .FirstOrDefault(i => i.Name == $"I{implementationType.Name}");

                if (interfaceType != null)
                {
                    services.AddScoped(interfaceType, implementationType);
                }
                else
                {
                    Log.Error("Servicio registrado sin interfaz: {Service}", implementationType.Name);
                }
            }
        }

        public static void ConfigurarBaseDeDatos(IServiceCollection services, string cadenaConexion)
        {
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseSqlServer(cadenaConexion).EnableSensitiveDataLogging(true),
                ServiceLifetime.Scoped);
        }
    }
}
