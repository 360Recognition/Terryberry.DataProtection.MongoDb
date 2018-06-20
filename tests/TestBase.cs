// Terryberry.DataProtection.MongoDb.Tests.TestBase.cs
// By Matthew DeJonge
// Email: mhdejong@umich.edu

namespace Terryberry.DataProtection.MongoDb.Tests
{
    using System.Xml.Linq;
    using MongoDB.Bson;
    using MongoDB.Driver;

    public abstract class TestBase
    {
        protected const string ApplicationName = "TerryberryDataProtectionTests";
        protected const string MongoDbUrl = "mongodb://localhost:27017";
        protected const string Database = "TerryberryDataProtectionTests";
        protected const string IdName = "id";
        protected const string Key = "<key id=\"3e44a364-9c6c-4b30-8c79-8ecfca124943\" version=\"1\"><creationDate>2018-04-12T15:15:54.9879433Z</creationDate><activationDate>2018-04-12T15:15:54.805328Z</activationDate><expirationDate>9999-12-31T00:00:00.00Z</expirationDate><descriptor deserializerType=\"Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel.AuthenticatedEncryptorDescriptorDeserializer, Microsoft.AspNetCore.DataProtection, Version=2.0.2.0, Culture=neutral, PublicKeyToken=adb9793829ddae60\"><descriptor><encryption algorithm=\"AES_256_CBC\" /><validation algorithm=\"HMACSHA256\" /><masterKey p4:requiresEncryption=\"true\" xmlns:p4=\"http://schemas.asp.net/2015/03/dataProtection\"><!-- Warning: the key below is in an unencrypted form. --><value>cBH6uO232L1JDUAX1VeFu+xBDd2uUqAv26pUA8fMtEpKN5PlVunICbq2uKEkmWirHoXgc1g1afojJ7hYoKJiiw==</value></masterKey></descriptor></descriptor></key>";

        protected TestBase(string collectionName)
        {
            KeyCollection = new MongoClient(MongoDbUrl).GetDatabase(Database).GetCollection<MongoDbXmlKey>(collectionName);
            KeyCollection.DeleteMany(FilterDefinition<MongoDbXmlKey>.Empty);
        }

        protected IMongoCollection<MongoDbXmlKey> KeyCollection { get; }

        protected void InsertKey(ObjectId mongodbId = default, string key = Key)
        {
            var xmlKey = new MongoDbXmlKey { Id = mongodbId, Key = key, KeyId = XElement.Parse(key).Attribute(IdName)?.Value };
            KeyCollection.InsertOne(xmlKey);
        }
    }
}
