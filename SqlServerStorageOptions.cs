using Forge.Security.Jwt.Service.Storage.SqlServer.Database;
using Microsoft.EntityFrameworkCore;

namespace Forge.Security.Jwt.Service.Storage.SqlServer
{

    /// <summary>Represents the option(s) for SqlServer storage</summary>
    public class SqlServerStorageOptions
    {

        /// <summary>Gets or sets the connection string.</summary>
        /// <value>The connection string.</value>
        public string ConnectionString { get; set; } = null!;

        /// <summary>Gets the DbContextOptionsBuilder.</summary>
        /// <value>The DbContextOptionsBuilder.</value>
        public DbContextOptionsBuilder<DatabaseContext> Builder { get; private set; } = new DbContextOptionsBuilder<DatabaseContext>();

    }

}
