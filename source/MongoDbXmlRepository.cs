namespace Terryberry.DataProtection.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using MongoDB.Driver;

    /// <summary>
    /// An xml repository backed by MongoDb.
    /// </summary>
    public class MongoDbXmlRepository : IXmlRepository
    {
        /// <summary>
        /// The name of the id attribute on the keys being inserted.
        /// </summary>
        private readonly string _idName;

        /// <summary>
        /// The collection to store keys in.
        /// </summary>
        private readonly IMongoCollection<MongoDbXmlKey> _keys;

        /// <summary>
        /// The key manager this app uses.
        /// </summary>
        private IKeyManager _keyManager;

        /// <summary>
        /// Initializes a <see cref="MongoDbXmlRepository" /> with keys stored in the specified MongoDb collection.
        /// </summary>
        /// <param name="keys">Collection used to store the keys.</param>
        /// <param name="idName">The name of the id attribute on the keys being inserted. Must be on the top level element.</param>
        public MongoDbXmlRepository(IMongoCollection<MongoDbXmlKey> keys, string idName)
        {
            _keys = keys;
            _idName = idName;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return _keys.Find(Builders<MongoDbXmlKey>.Filter.Empty).ToList().Select(key => key.XmlKey).ToList().AsReadOnly();
        }

        /// <inheritdoc />
        public void StoreElement(XElement element, string friendlyName)
        {
            RemoveRevokedKeys();
            _keys.InsertOne(new MongoDbXmlKey
            {
                XmlKey = element,
                KeyId = element.Attribute(_idName)?.Value
            });
        }

        /// <summary>
        /// Sets the key manager for cleanup.
        /// </summary>
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
            if (_keyManager is null) return;
            var activeKeys = _keyManager.GetAllKeys().Where(key => key.ExpirationDate > DateTimeOffset.Now && !key.IsRevoked).Select(key => key.KeyId.ToString());
            _keys.DeleteMany(Builders<MongoDbXmlKey>.Filter.Not(Builders<MongoDbXmlKey>.Filter.In(key => key.KeyId, activeKeys)));
        }
    }
}
