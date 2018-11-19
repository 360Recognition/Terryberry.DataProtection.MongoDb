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

### Release Notes

#### 1.0.0

* XML Repository for MongoDB.
* Data protection builders for MongoDB key persistence.

#### 2.0.0

* Improved key cleanup.
* Key cleanup is now an opt-in service (see basic usage).
* If you are upgrading from 1.0.0 existing keys may be removed prior to being revoked.
  * This should not be a problem if your keys are being automatically generated.
  * This can be avoided by adding a KeyId field (string) to each document with the value of the id attribute of the xml key.

#### 2.0.1

* Documentation updates.
* Minor performance improvements.
* Key cleanup now requires an id attribute on the top level element of keys (this is the default behavior).

#### 2.0.2

* Documentation updates.
* Minor performance improvements.
* MongoDB.Driver version increment.

### Open Source Licenses

#### Microsoft Frameworks and Libraries

<https://github.com/dotnet/standard/blob/master/LICENSE.TXT>\
<https://github.com/dotnet/core-setup/blob/master/LICENSE.TXT>\
<https://raw.githubusercontent.com/aspnet/Home/2.0.0/LICENSE.txt>\
<http://www.microsoft.com/web/webpi/eula/net_library_eula_enu.htm>

#### MongoDB.Driver

<http://www.apache.org/licenses/LICENSE-2.0>

#### xunit

<https://raw.githubusercontent.com/xunit/xunit/master/license.txt>
