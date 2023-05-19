using Forge.Security.Jwt.Shared.Service.Models;
using Forge.Security.Jwt.Shared.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Forge.Security.Jwt.Shared.Serialization;
using Forge.Security.Jwt.Service.Storage.SqlServer.Database;

namespace Forge.Security.Jwt.Service.Storage.SqlServer
{

    /// <summary>Service Collection Extension methods</summary>
    public static class ServiceCollectionExtension
    {

        /// <summary>
        /// Registers the Forge Jwt Service side SqlServer storage services.
        /// </summary>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddForgeJwtServiceSqlServerStorage(this IServiceCollection services, Action<SqlServerStorageOptions> configure)
        {
            return services
                .AddSingleton<ISerializationProvider, SystemTextJsonSerializer>()
                .Replace(new ServiceDescriptor(typeof(IStorage<JwtRefreshToken>), typeof(SqlServerStorage), ServiceLifetime.Singleton))
                .Configure<SqlServerStorageOptions>(configureOptions =>
                {
                    configure?.Invoke(configureOptions);
                    configureOptions.Builder.UseSqlServer(configureOptions.ConnectionString);
                    using (DatabaseContext context = new DatabaseContext(configureOptions.Builder.Options))
                    {
                        context.Database.Migrate();
                    }
                });
        }

    }

}
