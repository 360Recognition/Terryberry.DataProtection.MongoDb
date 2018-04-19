namespace Terryberry.DataProtection.MongoDb
{
    using System;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;

    /// <summary>
    /// Contains MongoDb-specific extension methods for modifying a <see cref="IDataProtectionBuilder"/>.
    /// </summary>
    public static class MongoDbDataProtectionBuilderExtensions
    {
        /// <summary>
        /// Configures the data protection system to persist keys to the specified database and collection in MongoDb.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="mongoUrl">MongoDb connection url.</param>
        /// <param name="database">Database used to store the key list.</param>
        /// <param name="collection">Collection used to store the key list.</param>
        /// <param name="removeExpiredKeys">If true, keys will eventually be removed after they expire.</param>
        /// <returns>A reference to the <see cref="IDataProtectionBuilder" /> after this operation has completed.</returns>
        public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder, string mongoUrl, string database, string collection, bool removeExpiredKeys = true)
        {
            return builder.PersistKeysToMongoDb(new MongoClient(mongoUrl).GetDatabase(database).GetCollection<MongoDbXmlKey>(collection), removeExpiredKeys);
        }

        /// <summary>
        /// Configures the data protection system to persist keys to the specified database and collection in MongoDb.
        /// </summary>
        /// <param name="builder">The builder instance to modify.</param>
        /// <param name="collection">Collection used to store the key list.</param>
        /// <param name="removeExpiredKeys">If true, keys will eventually be removed after they expire.</param>
        /// <returns>A reference to the <see cref="IDataProtectionBuilder" /> after this operation has completed.</returns>
        public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder, IMongoCollection<MongoDbXmlKey> collection, bool removeExpiredKeys = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (collection == null)
            {
                throw new ArgumentException(nameof(collection));
            }

            builder.Services.Configure<KeyManagementOptions>(options =>
            {
                options.XmlRepository = new MongoDbXmlRepository(collection, removeExpiredKeys);
            });

            return builder;
        }
    }
}
