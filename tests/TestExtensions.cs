namespace Terryberry.DataProtection.MongoDb.Tests;

using System;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;

public static class TestExtensions
{
    public static IKeyManager GetKeyManager(this IServiceProvider services)
    {
        return services.GetService<IKeyManager>() ?? throw new Exception("Failed to get KeyManager.");
    }
}
