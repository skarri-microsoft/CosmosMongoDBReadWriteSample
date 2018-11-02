namespace CosmosMongoDBReadWriteSample
{
    using MongoDB.Bson.Serialization.Attributes;
    using MongoDB.Bson.Serialization.IdGenerators;

    public class DocModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("Id")]
        public string Id{ get; set; }

        [BsonElement("index")]
        public int Index { get; set; }

        [BsonElement("guid")]
        public string Guid { get; set; }

        [BsonElement("isActive")]
        public bool IsActive { get; set; }

        [BsonElement("balance")]
        public string Balance { get; set; }

        [BsonElement("picture")]
        public string Picture { get; set; }

        [BsonElement("age")]
        public int Age { get; set; }

        [BsonElement("eyeColor")]
        public string EyeColor { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("gender")]
        public string Gender { get; set; }

        [BsonElement("company")]
        public string Company { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("about")]
        public string About { get; set; }

        [BsonElement("registered")]
        public string Registered { get; set; }

        [BsonElement("latitude")]
        public decimal Latitude { get; set; }

        [BsonElement("longitude")]
        public decimal Longitude { get; set; }

        [BsonElement("tags")]
        public string[] Tags { get; set; }

        [BsonElement("friends")]
        public Friend[] Friends { get; set; }

        [BsonElement("greeting")]
        public string Greeting { get; set; }

        [BsonElement("favoriteFruit")]
        public string FavoriteFruit { get; set; }
    }

    public class Friend
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("Id")]
        public int Id{ get; set; }

        [BsonElement("name")]
        public string Name { get; set; }
    }

}
