namespace Terryberry.DataProtection.MongoDb;

using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

/// <summary>
/// Contains MongoDB-specific extension methods for modifying a <see cref="IDataProtectionBuilder"/>.
/// </summary>
public static class MongoDbDataProtectionBuilderExtensions
{
    /// <summary>
    /// Configures the data protection system to persist keys to the specified database and collection in MongoDB.
    /// </summary>
    /// <param name="builder">The builder instance to modify.</param>
    /// <param name="connectionString">MongoDB connection url.</param>
    /// <param name="databaseName">Database used to store the key list.</param>
    /// <param name="collectionName">Collection used to store the key list.</param>
    /// <returns>A reference to the <see cref="IDataProtectionBuilder"/> after this operation has completed.</returns>
    public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder, string connectionString, string databaseName, string collectionName)
    {
        return builder.PersistKeysToMongoDb(new MongoClient(connectionString).GetDatabase(databaseName), collectionName);
    }

    /// <summary>
    /// Configures the data protection system to persist keys to the specified database and collection in MongoDB.
    /// </summary>
    /// <param name="builder">The builder instance to modify.</param>
    /// <param name="database">Database used to store the key list.</param>
    /// <param name="collectionName">Collection used to store the key list.</param>
    /// <returns>A reference to the <see cref="IDataProtectionBuilder"/> after this operation has completed.</returns>
    public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder, IMongoDatabase database, string collectionName)
    {
        if (database is null)
        {
            throw new ArgumentNullException(nameof(database));
        }
        return builder.PersistKeysToMongoDb(database.GetCollection<MongoDbXmlKey>(collectionName));
    }

    /// <summary>
    /// Configures the data protection system to persist keys to the specified database and collection in MongoDB.
    /// </summary>
    /// <param name="builder">The builder instance to modify.</param>
    /// <param name="collection">Collection used to store the key list.</param>
    /// <returns>A reference to the <see cref="IDataProtectionBuilder"/> after this operation has completed.</returns>
    public static IDataProtectionBuilder PersistKeysToMongoDb(this IDataProtectionBuilder builder, IMongoCollection<MongoDbXmlKey> collection)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        if (collection is null)
        {
            throw new ArgumentNullException(nameof(collection));
        }
        builder.Services.Configure<KeyManagementOptions>(options =>
        {
            var mongoDbXmlRepository = new MongoDbXmlRepository(collection);
            options.XmlRepository = mongoDbXmlRepository;
        });
        return builder;
    }

    /// <summary>
    /// Removes keys from the MongoDB repository after they expire or are revoked.
    /// </summary>
    /// <param name="builder">The builder instance to modify.</param>
    /// <param name="keyManager">Optional: <see cref="IKeyManager"/> to use. Default is whatever key manager is currently registered at the time this method is called.</param>
    /// <returns>A reference to the <see cref="IDataProtectionBuilder"/> after this operation has completed.</returns>
    /// <exception cref="InvalidOperationException">PersistKeysToMongoDb must be called before this method.</exception>
    /// <remarks>
    /// Cleanup will run after this method is called, and whenever the key manager calls <see cref="Microsoft.AspNetCore.DataProtection.Repositories.IXmlRepository.StoreElement"/>.
    /// </remarks>
    /// <remarks>
    /// If a custom key manager is used, it must be configured before calling this method and keys must have an "id" attribute on the top level element.
    /// </remarks>
    public static IDataProtectionBuilder AddKeyCleanup(this IDataProtectionBuilder builder, IKeyManager keyManager = null)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }
        keyManager ??= builder.Services.BuildServiceProvider().GetService<IKeyManager>();
        builder.Services.PostConfigure<KeyManagementOptions>(options =>
        {
            if (options.XmlRepository is MongoDbXmlRepository mongodbXmlRepository)
            {
                mongodbXmlRepository.SetKeyManager(keyManager);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(PersistKeysToMongoDb)} must be called before {nameof(AddKeyCleanup)}.");
            }
        });
        return builder;
    }
}
