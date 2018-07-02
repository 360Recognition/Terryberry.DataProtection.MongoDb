// Terryberry.DataProtection.MongoDb.Tests.KeyCollectionTests.cs
// By Matthew DeJonge
// Email: mhdejong@umich.edu

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
        private const string Key = "<key id=\"3e44a364-9c6c-4b30-8c79-8ecfca124943\" version=\"1\"><creationDate>2018-04-12T15:15:54.9879433Z</creationDate><activationDate>2018-04-12T15:15:54.805328Z</activationDate><expirationDate>9999-12-31T00:00:00.00Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=2.0.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>cBH6uO232L1JDUAX1VeFu+xBDd2uUqAv26pUA8fMtEpKN5PlVunICbq2uKEkmWirHoXgc1g1afojJ7hYoKJiiw==</value></masterKey></descriptor></descriptor></key>";

        public KeyCollectionTests() : base(nameof(KeyCollectionTests), nameof(KeyCollectionTests)) { }

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

            foreach (var key in keys) repository.StoreElement(key, null);

            var allDocuments = KeyCollection.Find(FilterDefinition<MongoDbXmlKey>.Empty).ToList();

            Assert.Equal(keys.Count, allDocuments.Count);

            Assert.Single(allDocuments, document => document.Key == Key);
        }
        
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
    }
}
