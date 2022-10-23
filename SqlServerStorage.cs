using Forge.Security.Jwt.Service.Storage.SqlServer.Database;
using Forge.Security.Jwt.Service.Storage.SqlServer.Models;
using Forge.Security.Jwt.Shared.Serialization;
using Forge.Security.Jwt.Shared.Service.Models;
using Forge.Security.Jwt.Shared.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Forge.Security.Jwt.Service.Storage.SqlServer
{

    /// <summary>SqlServer storage to persist tokens</summary>
    public class SqlServerStorage : IStorage<JwtRefreshToken>
    {

        private readonly ISerializationProvider _serializationProvider;
        private readonly SqlServerStorageOptions _options;

        /// <summary>Initializes a new instance of the <see cref="SqlServerStorage" /> class.</summary>
        /// <param name="serializationProvider">The serialization provider.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.ArgumentNullException">serviceProvider
        /// or
        /// serializationProvider</exception>
        public SqlServerStorage(ISerializationProvider serializationProvider, 
            IOptions<SqlServerStorageOptions> options)
        {
            if (serializationProvider == null) throw new ArgumentNullException(nameof(serializationProvider));
            if (options == null) throw new ArgumentNullException(nameof(options));
            _serializationProvider = serializationProvider;
            _options = options.Value;
        }

        /// <summary>Clears items from the storage</summary>
        public async Task ClearAsync()
        {
            using (DatabaseContext dbContext = Create())
            {
                dbContext.RemoveRange(GetTokens(dbContext));
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        /// <summary>Determines whether the specified key exist or not.</summary>
        /// <param name="key">The key.</param>
        /// <returns>
        ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> ContainsKeyAsync(string key)
        {
            using (DatabaseContext dbContext = Create())
            {
                return GetToken(dbContext, key) != null;
            }
        }

        /// <summary>Gets stored data</summary>
        /// <returns>List of data</returns>
        public async Task<IEnumerable<JwtRefreshToken>> GetAsync()
        {
            List<JwtRefreshToken> result = new List<JwtRefreshToken>();
            using (DatabaseContext dbContext = Create())
            {
                GetTokens(dbContext).ForEach(token =>
                {
                    result.Add(_serializationProvider.Deserialize<JwtRefreshToken>(token.Value));
                });
            }
            return result;
        }

        /// <summary>Gets the item by key</summary>
        /// <param name="key">The key.</param>
        /// <returns>Data or default</returns>
        public async Task<JwtRefreshToken> GetAsync(string key)
        {
            JwtRefreshToken result = null;
            using (DatabaseContext dbContext = Create())
            {
                Token token = GetToken(dbContext, key);
                if (token != null)
                {
                    result = _serializationProvider.Deserialize<JwtRefreshToken>(token.Value);
                }
            }
            return result;
        }

        /// <summary>Removes an item from the storage</summary>
        /// <param name="key">The key.</param>
        /// <returns>True, if it was successful, otherwise, False.</returns>
        public async Task<bool> RemoveAsync(string key)
        {
            bool result = false;
            using (DatabaseContext dbContext = Create())
            {
                Token token = GetToken(dbContext, key);
                if (token != null)
                {
                    dbContext.Remove(token);
                    result = true;
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                }
            }
            return result;
        }

        /// <summary>Add or update an item</summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        public async Task SetAsync(string key, JwtRefreshToken data)
        {
            using (DatabaseContext dbContext = Create())
            {
                Token token = new Token() { Id = key, Value = _serializationProvider.Serialize(data) };
                dbContext.Tokens.Add(token);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private DatabaseContext Create()
        {
            return new DatabaseContext(_options.Builder.Options);
        }

        private List<Token> GetTokens(DatabaseContext dbContext)
        {
            var query = from token in dbContext.Tokens
                        select token;
            return query.ToList();
        }

        private Token GetToken(DatabaseContext dbContext, string key)
        {
            Token result = null;
            var query = from token in dbContext.Tokens
                        where token.Id == key
                        select token;
            List<Token> tokens = query.Take(1).ToList();
            if (tokens.Count > 0) result = tokens[0];
            return result;
        }

    }

}