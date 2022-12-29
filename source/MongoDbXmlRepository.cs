namespace Terryberry.DataProtection.MongoDb;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using MongoDB.Driver;
using static MongoDB.Driver.Builders<MongoDbXmlKey>;

/// <summary>
/// An XML repository backed by MongoDB.
/// </summary>
public class MongoDbXmlRepository : IXmlRepository
{
    /// <summary>
    /// The collection to store keys in.
    /// </summary>
    private readonly IMongoCollection<MongoDbXmlKey> _keyCollection;

    /// <summary>
    /// Factory function for getting the key manager.
    /// </summary>
    private readonly Func<IKeyManager> _keyManagerFactory;

    /// <summary>
    /// Flag for enabling or disabling expired and revoked keys.
    /// </summary>
    private bool _cleanupKeys;

    /// <summary>
    /// Initializes a new instance <see cref="MongoDbXmlRepository"/> with keys stored in the specified MongoDB collection.
    /// </summary>
    /// <param name="keyCollection">Collection used to store the keys.</param>
    /// <param name="keyManagerFactory">Factory for the <see cref="IKeyManager"/>.</param>
    public MongoDbXmlRepository(IMongoCollection<MongoDbXmlKey> keyCollection, Func<IKeyManager> keyManagerFactory)
    {
        _keyCollection = keyCollection;
        _keyManagerFactory = keyManagerFactory;
    }

    /// <summary>
    /// Enables key cleanup.
    /// </summary>
    internal void EnableKeyCleanup()
    {
        _cleanupKeys = true;
    }

    /// <summary>
    /// Removes expired and revoked keys from the repository.
    /// </summary>
    private void RemoveRevokedKeys()
    {
        if (_cleanupKeys)
        {
            var keyManager = _keyManagerFactory();
            var activeKeys = keyManager.GetAllKeys().Where(key => key.ExpirationDate.ToUniversalTime() > DateTimeOffset.UtcNow && !key.IsRevoked);
            _keyCollection.DeleteMany(Filter.Nin(key => key.KeyId, activeKeys.Select(key => key.KeyId.ToString())));
        }
    }

    /// <summary>
    /// Gets all top-level XML elements in the repository.
    /// </summary>
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return _keyCollection.Find(Filter.Empty).Project(document => document.Key).ToList().Select(XElement.Parse).ToList();
    }

    /// <summary>
    /// Adds a top-level XML element to the repository.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <param name="friendlyName">A friendly name provided by the key manager. Not used in this method.</param>
    public void StoreElement(XElement element, string friendlyName)
    {
        _keyCollection.InsertOne(new MongoDbXmlKey(element));
        RemoveRevokedKeys();
    }
}
