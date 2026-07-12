using Forge.Security.Jwt.Service.Storage.SqlServer.Database;
using Forge.Security.Jwt.Service.Storage.SqlServer.Models;
using Forge.Security.Jwt.Shared.Serialization;
using Forge.Security.Jwt.Shared.Service.Models;
using Forge.Security.Jwt.Shared.Storage;
using Microsoft.EntityFrameworkCore;
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
            if (serializationProvider is null) throw new ArgumentNullException(nameof(serializationProvider));
            if (options is null) throw new ArgumentNullException(nameof(options));

            _serializationProvider = serializationProvider;
            _options = options.Value;
        }

        /// <summary>Clears items from the storage</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            using (DatabaseContext dbContext = Create())
            {
                dbContext.RemoveRange(await GetTokensAsync(dbContext, cancellationToken).ConfigureAwait(false));
                await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>Determines whether the specified key exist or not.</summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        ///   <c>true</c> if the specified key exists; otherwise, <c>false</c>.</returns>
        public async Task<bool> ContainsKeyAsync(string key, CancellationToken cancellationToken = default)
        {
            using (DatabaseContext dbContext = Create())
            {
                return await GetTokenAsync(dbContext, key, cancellationToken).ConfigureAwait(false) is not null;
            }
        }

        /// <summary>Gets stored data</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of data</returns>
        public async Task<IEnumerable<JwtRefreshToken>> GetAsync(CancellationToken cancellationToken = default)
        {
            List<JwtRefreshToken> result = new();

            using (DatabaseContext dbContext = Create())
            {
                (await GetTokensAsync(dbContext, cancellationToken).ConfigureAwait(false)).ForEach(token =>
                {
                    result.Add(_serializationProvider.Deserialize<JwtRefreshToken>(token.Value));
                });
            }

            return result;
        }

        /// <summary>Gets the item by key</summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Data or default</returns>
        public async Task<JwtRefreshToken> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            JwtRefreshToken? result = null;

            using (DatabaseContext dbContext = Create())
            {
                Token? token = await GetTokenAsync(dbContext, key, cancellationToken).ConfigureAwait(false);
                if (token is not null)
                {
                    result = _serializationProvider.Deserialize<JwtRefreshToken>(token.Value);
                }
            }

            return result!;
        }

        /// <summary>Removes an item from the storage</summary>
        /// <param name="key">The key.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>True, if it was successful, otherwise, False.</returns>
        public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            bool result = false;

            using (DatabaseContext dbContext = Create())
            {
                Token? token = await GetTokenAsync(dbContext, key, cancellationToken).ConfigureAwait(false);
                if (token != null)
                {
                    dbContext.Remove(token);
                    result = true;
                    await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            return result;
        }

        /// <summary>Add or update an item</summary>
        /// <param name="key">The key.</param>
        /// <param name="data">The data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task SetAsync(string key, JwtRefreshToken data, CancellationToken cancellationToken = default)
        {
            // The primary key on Token.Id is the source of truth across all app instances,
            // so the read-then-write below is not atomic: a concurrent SetAsync/RemoveAsync
            // (even from another process) may insert or delete the same key between our read
            // and our SaveChanges. On such a conflict we discard the context, re-read and retry,
            // so the operation converges to a correct upsert instead of throwing.
            const int maxAttempts = 3;

            string serializedValue = _serializationProvider.Serialize(data);

            for (int attempt = 1; ; attempt++)
            {
                using (DatabaseContext dbContext = Create())
                {
                    Token? existingToken = await GetTokenAsync(dbContext, key, cancellationToken).ConfigureAwait(false);

                    if (existingToken != null)
                    {
                        existingToken.Value = serializedValue;
                        dbContext.Tokens.Update(existingToken);
                    }
                    else
                    {
                        dbContext.Tokens.Add(new Token() { Id = key, Value = serializedValue });
                    }

                    try
                    {
                        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        return;
                    }
                    catch (DbUpdateException) when (attempt < maxAttempts)
                    {
                        // Insert conflict (another writer created the row) or an update whose row
                        // was concurrently deleted (DbUpdateConcurrencyException). Retry with a
                        // fresh read; the next pass takes the correct branch. After maxAttempts the
                        // exception is allowed to propagate.
                    }
                }
            }
        }

        private DatabaseContext Create()
        {
            return new DatabaseContext(_options.Builder.Options);
        }

        private static Task<List<Token>> GetTokensAsync(DatabaseContext dbContext, CancellationToken cancellationToken = default)
        {
            IQueryable<Token> query = from token in dbContext.Tokens
                                      select token;

            return query.ToListAsync(cancellationToken);
        }

        private static async Task<Token?> GetTokenAsync(DatabaseContext dbContext, string key, CancellationToken cancellationToken = default)
        {
            Token? result = null;

            IQueryable<Token> query = from token in dbContext.Tokens
                                      where token.Id == key
                                      select token;

            List<Token> tokens = await query
                .Take(1)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            if (tokens.Count > 0) result = tokens[0];

            return result;
        }

    }

}