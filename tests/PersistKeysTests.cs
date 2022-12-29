namespace Terryberry.DataProtection.MongoDb.Tests;

using System;
using System.Linq;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class PersistKeysTests : TestBase
{
    private const string Name = nameof(PersistKeysTests);

    public PersistKeysTests() : base(Name) { }

    [Fact]
    public void PersistKeysToMongoDb()
    {
        var services = new ServiceCollection();
        services.AddDataProtection()
                .PersistKeysToMongoDb("mongodb://localhost:27017", Name, Name)
                .AddKeyCleanup()
                .SetApplicationName(Name);

        var serviceProvider = services.BuildServiceProvider();
        var keyManager = serviceProvider.GetKeyManager();

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
                .SetApplicationName(Name);

        var serviceProvider = services.BuildServiceProvider();
        var keyManager = serviceProvider.GetKeyManager();

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
                .SetApplicationName(Name);

        var serviceProvider = services.BuildServiceProvider();
        var keyManager = serviceProvider.GetKeyManager();

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
                .SetApplicationName(Name);

        var serviceProvider = services.BuildServiceProvider();
        var keyManager = serviceProvider.GetKeyManager();

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
                .PersistKeysToMongoDb(KeyCollection.Database, Name)
                .AddKeyCleanup()
                .SetApplicationName(Name);

        var serviceProvider = services.BuildServiceProvider();
        var keyManager = serviceProvider.GetKeyManager();

        var key = keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);
        keyManager.RevokeAllKeys(DateTimeOffset.MaxValue);
        keyManager.CreateNewKey(DateTimeOffset.UtcNow, DateTimeOffset.MaxValue);

        var allKeys = keyManager.GetAllKeys();
        var allKeyIds = allKeys.Select(k => k.KeyId).ToList();

        Assert.DoesNotContain(key.KeyId, allKeyIds);
        Assert.Equal(1, allKeys.Count);
    }
}
