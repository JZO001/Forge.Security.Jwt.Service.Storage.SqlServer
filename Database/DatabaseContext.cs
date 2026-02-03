using Forge.Security.Jwt.Service.Storage.SqlServer.Models;
using Microsoft.EntityFrameworkCore;

namespace Forge.Security.Jwt.Service.Storage.SqlServer.Database
{

    /// <summary>Represents the database context</summary>
    public class DatabaseContext : DbContext
    {

        /// <summary>Initializes a new instance of the <see cref="DatabaseContext" /> class.</summary>
        public DatabaseContext() : base(CreateOptions())
        {
        }

        /// <summary>Initializes a new instance of the <see cref="DatabaseContext" /> class.</summary>
        /// <param name="options">The options for this context.</param>
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }

        /// <summary>Gets or sets the default connection string.</summary>
        /// <value>The default connection string.</value>
#if DEBUG
        public static string DefaultConnectionString { get; set; } = "Data Source=.\\SQLEXPRESS2019;Initial Catalog=ForgeJwtServiceStorage;Integrated Security=True;TrustServerCertificate=True";
#else
        public static string DefaultConnectionString { get; set; } = "";
#endif

        private static DbContextOptions<DatabaseContext> CreateOptions()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseSqlServer(DefaultConnectionString);
            return optionsBuilder.Options;
        }

        /// <summary>Gets or sets the tokens.</summary>
        /// <value>The tokens.</value>
        public DbSet<Token> Tokens { get; set; }

    }

}
