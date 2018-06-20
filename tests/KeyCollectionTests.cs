// Terryberry.DataProtection.MongoDb.Tests.KeyCollectionTests.cs
// By Matthew DeJonge
// Email: mhdejong@umich.edu

namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System;
    using System.Linq;
    using System.Xml.Linq;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Xunit;

    public class KeyCollectionTests : TestBase
    {
        public KeyCollectionTests() : base(nameof(KeyCollectionTests)) { }

        [Fact]
        public void TestGetAllElements()
        {
            var id = ObjectId.GenerateNewId();

            InsertKey(id);

            var count = InsertKeys(key => InsertKey(ObjectId.GenerateNewId(), key.ToString(SaveOptions.DisableFormatting)));

            var repository = new MongoDbXmlRepository(KeyCollection);

            var allElements = repository.GetAllElements().ToList();

            Assert.Equal(count + 1, allElements.Count);

            Assert.Single(allElements.Where(element => element.ToString(SaveOptions.DisableFormatting) == Key));

            Assert.Single(KeyCollection.Find(document => document.Id == id).ToEnumerable());
        }

        [Fact]
        public void TestStoreElement()
        {
            var repository = new MongoDbXmlRepository(KeyCollection);

            var count = InsertKeys(key => repository.StoreElement(key, null));

            repository.StoreElement(XElement.Parse(Key), null);

            var allDocuments = KeyCollection.Find(FilterDefinition<MongoDbXmlKey>.Empty).ToList();

            Assert.Equal(count + 1, allDocuments.Count);

            Assert.Single(allDocuments.Where(document => document.Key == Key));
        }

        private static int InsertKeys(Action<XElement> insertKey)
        {
            var random = new Random();

            var count = random.Next(5, 20);

            for (var i = 0; i < count; i++)
            {
                var id = Guid.NewGuid();
                var key = XElement.Parse(Key);
                key.SetAttributeValue(IdName, id);
                insertKey(key);
            }

            return count;
        }
    }
}
