namespace Terryberry.DataProtection.MongoDb.Tests;

using System;
using MongoDB.Driver;

public abstract class TestBase : IDisposable
{
    protected TestBase(string name)
    {
        KeyCollection = new MongoClient().GetDatabase(name).GetCollection<MongoDbXmlKey>(name);
    }

    protected IMongoCollection<MongoDbXmlKey> KeyCollection { get; }

    public void Dispose()
    {
        KeyCollection.Database.Client.DropDatabase(KeyCollection.Database.DatabaseNamespace.DatabaseName);
    }
}
