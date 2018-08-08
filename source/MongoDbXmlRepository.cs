namespace Terryberry.DataProtection.MongoDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using MongoDB.Driver;
    using static MongoDB.Driver.Builders<MongoDbXmlKey>;

    /// <summary>
    /// An xml repository backed by MongoDb.
    /// </summary>
    public class MongoDbXmlRepository : IXmlRepository
    {
        /// <summary>
        /// The name of the id attribute on the keys.
        /// </summary>
        private const string Id = "id";

        /// <summary>
        /// The collection to store keys in.
        /// </summary>
        private readonly IMongoCollection<MongoDbXmlKey> _keys;

        /// <summary>
        /// The key manager this app uses.
        /// </summary>
        private IKeyManager _keyManager;

        /// <summary>
        /// Initializes a <see cref="MongoDbXmlRepository"/> with keys stored in the specified MongoDb collection.
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
            if (_keyManager is null) return;
            var activeKeys = _keyManager.GetAllKeys().Where(key => key.ExpirationDate > DateTimeOffset.Now && !key.IsRevoked).Select(key => key.KeyId.ToString());
            _keys.DeleteMany(Filter.Nin(key => key.KeyId, activeKeys));
        }

        /// <summary>
        /// Gets all top-level XML elements in the repository.
        /// </summary>
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return _keys.Find(Filter.Empty).ToList().Select(key => key.XmlKey).ToList();
        }

        /// <summary>
        /// Adds a top-level XML element to the repository.
        /// </summary>
        /// <param name="element">The element to add.</param>
        /// <param name="friendlyName">A friendly name provided by the key mananger. Not used in this method.</param>
        public void StoreElement(XElement element, string friendlyName)
        {
            RemoveRevokedKeys();
            _keys.InsertOne(new MongoDbXmlKey
            {
                XmlKey = element,
                KeyId = element.Attribute(Id)?.Value
            });
        }
    }
}
