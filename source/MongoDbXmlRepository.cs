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
    private readonly IMongoCollection<MongoDbXmlKey> _keys;

    /// <summary>
    /// The key manager this app uses.
    /// </summary>
    private IKeyManager _keyManager;

    /// <summary>
    /// Initializes a new instance <see cref="MongoDbXmlRepository"/> with keys stored in the specified MongoDB collection.
    /// </summary>
    /// <param name="keys">Collection used to store the keys.</param>
    public MongoDbXmlRepository(IMongoCollection<MongoDbXmlKey> keys)
    {
        _keys = keys;
    }

    /// <summary>
    /// Sets the key manager for cleanup.
    /// </summary>
    /// <param name="keyManager">The <see cref="IKeyManager"/>.</param>
    internal void SetKeyManager(IKeyManager keyManager)
    {
        _keyManager = keyManager;
        RemoveRevokedKeys();
    }

    /// <summary>
    /// Removes expired and revoked keys from the repository.
    /// </summary>
    private void RemoveRevokedKeys()
    {
        if (_keyManager is null)
        {
            return;
        }
        var activeKeys = _keyManager.GetAllKeys().Where(key => key.ExpirationDate.ToUniversalTime() > DateTimeOffset.UtcNow && !key.IsRevoked);
        _keys.DeleteMany(Filter.Nin(key => key.KeyId, activeKeys.Select(key => key.KeyId.ToString())));
    }

    /// <summary>
    /// Gets all top-level XML elements in the repository.
    /// </summary>
    public IReadOnlyCollection<XElement> GetAllElements()
    {
        return _keys.Find(Filter.Empty).Project(document => document.Key).ToList().Select(XElement.Parse).ToList();
    }

    /// <summary>
    /// Adds a top-level XML element to the repository.
    /// </summary>
    /// <param name="element">The element to add.</param>
    /// <param name="friendlyName">A friendly name provided by the key manager. Not used in this method.</param>
    public void StoreElement(XElement element, string friendlyName)
    {
        RemoveRevokedKeys();
        _keys.InsertOne(new MongoDbXmlKey(element));
    }
}
