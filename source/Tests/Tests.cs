namespace Tests
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MongoDB.Driver;
    using Terryberry.DataProtection.MongoDb;

    [TestClass]
    public class Tests
    {
        private const string MongoDbUrl = "mongodb://localhost:27017";
        private const string Database = "TerryberryDataProtectionTests";
        private const string Collection = "AntiforgeryKeys";

        private const string ActiveKey = "<key id=\"3E44A364-9C6C-4B30-8C79-8ECFCA124943\" version=\"1\"><creationDate>2018-04-12T15:15:54.9879433Z</creationDate><activationDate>2018-04-12T15:15:54.805328Z</activationDate><expirationDate>9999-12-31T00:00:00.00Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=2.0.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>cBH6uO232L1JDUAX1VeFu+xBDd2uUqAv26pUA8fMtEpKN5PlVunICbq2uKEkmWirHoXgc1g1afojJ7hYoKJiiw==</value></masterKey></descriptor></descriptor></key>";
        private const string ExpiredKey = "<key id=\"53A942D0-E8BF-4DAB-983A-249F6E487F7D\" version=\"1\"><creationDate>2017-04-12T15:15:54.9879433Z</creationDate><activationDate>2017-04-12T15:15:54.805328Z</activationDate><expirationDate>2017-07-11T15:15:54.805328Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=2.0.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>cBH6uO232L1JDUAX1VeFu+xBDd2uUqAv26pUA8fMtEpKN5PlVunICbq2uKEkmWirHoXgc1g1afojJ7hYoKJiiw==</value></masterKey></descriptor></descriptor></key>";
        
        public Tests()
        {
            KeyCollection = new MongoClient(MongoDbUrl).GetDatabase(Database).GetCollection<MongoDbXmlKey>(Collection);
        }

        private IMongoCollection<MongoDbXmlKey> KeyCollection { get; }

        [TestMethod]
        public void TestGetAllElements()
        {
            DeleteAllKeys();
            InsertActiveKey();

            var repository = new MongoDbXmlRepository(KeyCollection, true);
            var allElements = repository.GetAllElements();

            Assert.AreEqual(1, allElements.Count);
        }

        [TestMethod]
        public void TestStoreElement()
        {
            DeleteAllKeys();

            var repository = new MongoDbXmlRepository(KeyCollection, true);
            repository.StoreElement(XElement.Parse(ActiveKey), null);
            var insertedDocument = KeyCollection.Find(FilterDefinition<MongoDbXmlKey>.Empty).Single();

            Assert.AreEqual(ActiveKey, insertedDocument.Key);
        }

        [TestMethod]
        public void TestPersistKeysToMongoDbExpiredKeyDoesNotGetRemoved()
        {
            DeleteAllKeys();
            InsertActiveKey();
            InsertExpiredKey();

            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(MongoDbUrl, Database, Collection, false)
                .SetApplicationName(Database);

            var keyManager = CreateNewKeyAndReturnKeyManager(services);

            var allKeys = keyManager.GetAllKeys();

            var expectedId = Guid.Parse(XElement.Parse(ActiveKey).FirstAttribute.Value);

            Assert.AreEqual(expectedId, allKeys.First().KeyId);
            Assert.AreEqual(3, allKeys.Count);
        }

        [TestMethod]
        public void TestPersistKeysToMongoDbExpiredKeyGetsRemoved()
        {
            DeleteAllKeys();
            InsertActiveKey();
            InsertExpiredKey();

            var services = new ServiceCollection();

            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(Database);
            
            var keyManager = CreateNewKeyAndReturnKeyManager(services);

            Assert.AreEqual(2, keyManager.GetAllKeys().Count);
        }

        private static IKeyManager CreateNewKeyAndReturnKeyManager(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            return keyManager;
        }

        private void InsertActiveKey()
        {
            KeyCollection.InsertOne(new MongoDbXmlKey { Id = default, Key = ActiveKey });
        }

        private void InsertExpiredKey()
        {
            KeyCollection.InsertOne(new MongoDbXmlKey { Id = default, Key = ExpiredKey });
        }

        private void DeleteAllKeys()
        {
            KeyCollection.DeleteMany(FilterDefinition<MongoDbXmlKey>.Empty);
        }
    }
}
