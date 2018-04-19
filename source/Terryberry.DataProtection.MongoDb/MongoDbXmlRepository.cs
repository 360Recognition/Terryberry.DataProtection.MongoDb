namespace Terryberry.DataProtection.MongoDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using MongoDB.Driver;

    /// <summary>
    /// An XML repository backed by MongoDb.
    /// </summary>
    public class MongoDbXmlRepository : IXmlRepository
    {
        /// <summary>
        /// The collection to store keys in.
        /// </summary>
        private readonly IMongoCollection<MongoDbXmlKey> _keyCollection;

        /// <summary>
        /// true if expired keys should be removed; otherwise, false.
        /// </summary>
        private readonly bool _removeExpiredKeys;

        /// <summary>
        /// Initializes a <see cref="MongoDbXmlRepository"/> with keys stored in the specified MongoDb collection.
        /// </summary>
        /// <param name="keyCollection">Collection used to store the key list.</param>
        /// <param name="removeExpiredKeys">If true, keys will eventually be removed after they expire.</param>
        public MongoDbXmlRepository(IMongoCollection<MongoDbXmlKey> keyCollection, bool removeExpiredKeys)
        {
            _keyCollection = keyCollection;
            _removeExpiredKeys = removeExpiredKeys;
        }

        /// <summary>
        /// Finalizes this <see cref="MongoDbXmlRepository" />.
        /// </summary>
        ~MongoDbXmlRepository()
        {
            RemoveExpiredKeys();
        }

        /// <inheritdoc />
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            return GetAllElementsCore().Select(key => key.XmlKey).ToList().AsReadOnly();
        }

        /// <summary>
        /// Gets all elements from the repository.
        /// </summary>
        /// <returns>All elements in the repository.</returns>
        private IEnumerable<MongoDbXmlKey> GetAllElementsCore()
        {
            return _keyCollection.Find(Builders<MongoDbXmlKey>.Filter.Empty).ToEnumerable();
        }

        /// <inheritdoc />
        public void StoreElement(XElement element, string friendlyName)
        {
            RemoveExpiredKeys();
            _keyCollection.InsertOne(new MongoDbXmlKey { XmlKey = element });
        }

        /// <summary>
        /// Removes expired keys from the repository.
        /// </summary>
        private void RemoveExpiredKeys()
        {
            if (!_removeExpiredKeys) return;
            var expiredKeys = GetAllElementsCore().Where(key => key.IsExpired).Select(key => key.Id);
            _keyCollection.DeleteMany(Builders<MongoDbXmlKey>.Filter.In(key => key.Id, expiredKeys));
        }
    }
}
