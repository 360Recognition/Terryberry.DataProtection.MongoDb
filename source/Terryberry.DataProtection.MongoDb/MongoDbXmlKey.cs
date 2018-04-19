namespace Terryberry.DataProtection.MongoDb
{
    using System;
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
        /// The antiforgery key xml in string form.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets the antiforgery key as an <see cref="XElement" />.
        /// </summary>
        [BsonIgnore]
        public XElement XmlKey
        {
            get => XElement.Parse(Key);
            set => Key = value.ToString(SaveOptions.DisableFormatting);
        } 

        /// <summary>
        /// Is this key expired?
        /// </summary>
        [BsonIgnore]
        public bool IsExpired => DateTimeOffset.Parse(XmlKey.Element("expirationDate")?.Value) < DateTimeOffset.Now;
    }
}
