namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class PersistKeysTests : TestBase
    {
        private const string ApplicationName = "TerryberryDataProtectionTests";
        private const string Database = nameof(PersistKeysTests);
        private const string Collection = nameof(PersistKeysTests);

        public PersistKeysTests() : base(Database, Collection) { }

        [Fact]
        public void PersistKeysToMongoDb()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb("mongodb://localhost:27017", Database, Collection)
                .AddKeyCleanup()
                .SetApplicationName(ApplicationName);

            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();

            var key = keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys();
            var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

            Assert.Contains(key.KeyId, allKeyIds);
            Assert.Equal(2, allKeys.Count);
        }

        [Fact]
        public void ExpiredKeyDoesNotGetRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(ApplicationName);

            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();

            var key = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MinValue);
            keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys();
            var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

            Assert.Contains(key.KeyId, allKeyIds);
            Assert.Equal(2, allKeys.Count);
        }

        [Fact]
        public void ExpiredKeyGetsRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .AddKeyCleanup()
                .SetApplicationName(ApplicationName);

            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();

            var key = keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MinValue);
            keyManager.CreateNewKey(DateTimeOffset.Now, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys();
            var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

            Assert.DoesNotContain(key.KeyId, allKeyIds);
            Assert.Equal(1, allKeys.Count);
        }

        [Fact]
        public void RevokedKeyDoesNotGetRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection)
                .SetApplicationName(ApplicationName);

            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();

            var key = keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            keyManager.RevokeAllKeys(DateTimeOffset.MaxValue);
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys();
            var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

            Assert.Contains(key.KeyId, allKeyIds);
            Assert.Equal(3, allKeys.Count);
        }

        [Fact]
        public void RevokedKeyGetsRemoved()
        {
            var services = new ServiceCollection();
            services.AddDataProtection()
                .PersistKeysToMongoDb(KeyCollection.Database, Collection)
                .AddKeyCleanup()
                .SetApplicationName(ApplicationName);

            var serviceProvider = services.BuildServiceProvider();
            var keyManager = serviceProvider.GetService<IKeyManager>();

            var key = keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
            keyManager.RevokeAllKeys(DateTimeOffset.MaxValue);
            keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);

            var allKeys = keyManager.GetAllKeys();
            var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

            Assert.DoesNotContain(key.KeyId, allKeyIds);
            Assert.Equal(1, allKeys.Count);
        }
    }
}
