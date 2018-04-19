# Terryberry.DataProtection.MongoDb

Data Protection APIs for persisting anti-forgery keys to MongoDb.

## Basic Useage

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddDataProtection()
        .SetApplicationName("MyApplication")
        .PersistKeysToMongoDb("mongodb://localhost:27017/", "KeyDatabase", "KeyCollection");
}
```

### Open Source Licenses

#### Microsoft Frameworks and Libraries

<https://github.com/dotnet/standard/blob/master/LICENSE.TXT>\
<https://github.com/dotnet/core-setup/blob/master/LICENSE.TXT>\
<https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt>\
<http://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm>

#### MongoDb.Driver

<http://www.apache.org/licenses/LICENSE-2.0>
