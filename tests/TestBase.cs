namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System;
    using MongoDB.Driver;

    public abstract class TestBase : IDisposable
    {
        protected TestBase(string database, string collection)
        {
            KeyCollection = new MongoClient().GetDatabase(database).GetCollection<MongoDbXmlKey>(collection);
        }

        protected IMongoCollection<MongoDbXmlKey> KeyCollection { get; }

        public void Dispose()
        {
            KeyCollection.Database.Client.DropDatabase(KeyCollection.Database.DatabaseNamespace.DatabaseName);
        }
    }
}
