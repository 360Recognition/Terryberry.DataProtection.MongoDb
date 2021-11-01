namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using Xunit;

    public class KeyCollectionTests : TestBase
    {
        private const string IdName = "id";
        private const string Key = "<key id=\"1\">mock_value</key>";

        public KeyCollectionTests() : base(nameof(KeyCollectionTests)) { }

        public static IEnumerable<object[]> GenerateKeys()
        {
            var keys = new List<XElement> { XElement.Parse(Key) };
            for (var i = 0; i < new Random().Next(5, 20); i++)
            {
                var key = XElement.Parse(Key);
                key.SetAttributeValue(IdName, Guid.NewGuid());
                keys.Add(key);
            }
            yield return new object[] { keys };
        }

        [Theory]
        [MemberData(nameof(GenerateKeys))]
        public void TestGetAllElements(List<XElement> keys)
        {
            var mongodbKeys = keys.Select(key => new MongoDbXmlKey
            {
                Id = ObjectId.GenerateNewId(),
                Key = key.ToString(SaveOptions.DisableFormatting),
                KeyId = key.Attribute(IdName)?.Value
            }).ToList();
            KeyCollection.InsertMany(mongodbKeys);
            var repository = new MongoDbXmlRepository(KeyCollection);
            var allElements = repository.GetAllElements();
            Assert.Equal(keys.Count, allElements.Count);
            Assert.Single(allElements, element => element.ToString(SaveOptions.DisableFormatting) == Key);
            Assert.Single(KeyCollection.Find(document => document.Id == mongodbKeys.First().Id).ToEnumerable());
        }

        [Theory]
        [MemberData(nameof(GenerateKeys))]
        public void TestStoreElement(List<XElement> keys)
        {
            var repository = new MongoDbXmlRepository(KeyCollection);
            foreach (var key in keys)
            {
                repository.StoreElement(key, null);
            }
            var allDocuments = KeyCollection.Find(FilterDefinition<MongoDbXmlKey>.Empty).ToList();
            Assert.Equal(keys.Count, allDocuments.Count);
            Assert.Single(allDocuments, document => document.Key == Key);
        }
    }
}
