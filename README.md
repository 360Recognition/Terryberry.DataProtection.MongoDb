# Terryberry.DataProtection.MongoDb

Data Protection APIs for persisting keys to MongoDB.

## Basic Usage

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
            .SetApplicationName("MyApplication")
            .PersistKeysToMongoDb("mongodb://localhost:27017/", "KeyDatabase", "KeyCollection")
            .AddKeyCleanup();
}
```
