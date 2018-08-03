namespace Terryberry.DataProtection.MongoDb
{
    using System.Xml.Linq;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Wraps the xml key in an object that can be stored in MongoDb.
    /// </summary>
    public class MongoDbXmlKey
    {
        /// <summary>
        /// MongoDb _id field.
        /// </summary>
        public ObjectId Id { get; set; }

        /// <summary>
        /// The key xml in string form.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The id from the xml key.
        /// </summary>
        [BsonIgnoreIfDefault]
        public string KeyId { get; set; }

        /// <summary>
        /// The key as an <see cref="XElement"/>.
        /// </summary>
        [BsonIgnore]
        public XElement XmlKey
        {
            get => XElement.Parse(Key);
            set => Key = value.ToString(SaveOptions.DisableFormatting);
        }
    }
}
