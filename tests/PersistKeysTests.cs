// Terryberry.DataProtection.MongoDb.Tests.PersistKeysTests.cs
// By Matthew DeJonge
// Email: mhdejong@umich.edu

namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.Extensions.DependencyInjection;
    using MongoDB.Driver;
    using Xunit;

    public class PersistKeysTests : TestBase
    {
        public PersistKeysTests() : base(nameof(PersistKeysTests)) { }

        [Fact]
        public void TestPersistKeysToMongoDb()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(MongoDbUrl, Database, nameof(PersistKeysTests))
                .AddKeyCleanup()
                .SetApplicationName(ApplicationName);

            AssertResult(CreateKeyManagerAndNewKey(services));
        }

        [Fact]
        public void TestPersistKeysToMongoDbRevokedKeyGetsRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection.Database, nameof(PersistKeysTests))
                .AddKeyCleanup()
                .SetApplicationName(ApplicationName);

            var keyManager = CreateKeyManagerAndNewKey(services);
            keyManager.RevokeAllKeys(DateTimeOffset.MaxValue);

            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            AssertResult(keyManager);
        }

        [Fact]
        public void TestPersistKeysToMongoDbRevokedKeyDoesNotGetRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(ApplicationName);

            var keyManager = CreateKeyManagerAndNewKey(services);
            keyManager.RevokeAllKeys(DateTimeOffset.MaxValue);
            AssertResult(keyManager);
        }

        [Fact]
        public void TestPersistKeysToMongoDbExpiredKeyGetsRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(ApplicationName)
                .AddKeyCleanup();

            var keyManager = CreateKeyManagerAndNewKey(services);
            keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MinValue);
            AssertResult(keyManager);
        }

        [Fact]
        public void TestPersistKeysToMongoDbExpiredKeyDoesNotGetRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(ApplicationName)
                .AddKeyCleanup();

            var keyManager = CreateKeyManagerAndNewKey(services);
            var newKey = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MinValue);
            KeyCollection.DeleteOne(key => key.KeyId == newKey.KeyId.ToString());
            AssertResult(keyManager);
        }

        private static IKeyManager CreateKeyManagerAndNewKey(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            return keyManager;
        }

        private void AssertResult(IKeyManager keyManager)
        {
            InsertKey();

            keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys().Select(key => key.KeyId).ToList();
            var expectedId = Guid.Parse(XElement.Parse(Key).Attribute(IdName)?.Value);

            Assert.Contains(expectedId, allKeys);
            Assert.Equal(3, keyManager.GetAllKeys().Count);
        }
    }
}
