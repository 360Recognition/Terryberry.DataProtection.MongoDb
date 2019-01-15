namespace Terryberry.DataProtection.MongoDb
{
    using System.Xml.Linq;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    /// <summary>
    /// Wraps the XML key in an object that can be stored in MongoDB.
    /// </summary>
    public class MongoDbXmlKey
    {
        /// <summary>
        /// The name of the id attribute on the keys.
        /// </summary>
        private const string IdAttribute = "id";

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbXmlKey"/>.
        /// </summary>
        public MongoDbXmlKey() { }

        /// <summary>
        /// Initializes a new instance of <see cref="MongoDbXmlKey"/> with the specified XML key.
        /// </summary>
        /// <param name="element">XML data protection key.</param>
        internal MongoDbXmlKey(XElement element)
        {
            Key = element.ToString(SaveOptions.DisableFormatting);
            KeyId = element.Attribute(IdAttribute)?.Value;
        }

        /// <summary>
        /// MongoDB _id field.
        /// </summary>
        [BsonId]
        public ObjectId Id { get; set; }

        /// <summary>
        /// The key XML in string form.
        /// </summary>
        [BsonRequired]
        public string Key { get; set; }

        /// <summary>
        /// The id from the XML key.
        /// </summary>
        [BsonIgnoreIfDefault]
        public string KeyId { get; set; }

        /// <summary>
        /// Parses this key as an <see cref="XElement"/>.
        /// </summary>
        [BsonIgnore]
        internal XElement ToXElement => XElement.Parse(Key);
    }
}
