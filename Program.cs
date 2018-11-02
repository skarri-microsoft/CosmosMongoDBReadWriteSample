namespace CosmosMongoDBReadWriteSample
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Authentication;
    using System.Threading.Tasks;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class Program
    {
        private static MongoClient mongoClient;
        private static string dbName = "Sampledb";
        private static string collectionName = "test4";
        private static IMongoDatabase database;
        private static IMongoCollection<DocModel> docStoreCollection;
        private static List<DocModel> sampleDocuments=new List<DocModel>();
        private static List<DocModel> receivedDocuments = new List<DocModel>();
        private static List<DocModel> InsertFailedDocs=new List<DocModel>();
        private static List<DocModel> ReadFailedDocs=new List<DocModel>();
        private static int DocsCount = 2500;
        private static int WriteBatchSize = 250;
        private static int InsertRetries = 3;
        private static int ReadRetries = 3;
        private static int MinWait = 1500;
        private static int MaxWait = 3000;


        static void Main(string[] args)
        {

            string connectionString =
               ConfigurationManager.AppSettings["conn"];

            MongoClientSettings settings = MongoClientSettings.FromUrl(
                new MongoUrl(connectionString)
            );
            settings.SslSettings =
                new SslSettings() { EnabledSslProtocols = SslProtocols.Tls12 };
            settings.MaxConnectionPoolSize = 6000;
            mongoClient = new MongoClient(settings);

            database = mongoClient.GetDatabase(dbName);

            docStoreCollection = database.GetCollection<DocModel>(collectionName);

            GenerateSampleDocuments().Wait();

            InsertAllDocuments().Wait();

            PrintLatestOperationRus();

            ReadAllDocuments().Wait();

            PrintLatestOperationRus();

            Console.WriteLine("Press enter to exit...");

            Console.ReadLine();
        }

        private static void PrintLatestOperationRus()
        {
            var result =database.RunCommand<BsonDocument>(new BsonDocument { { "getLastRequestStatistics", 1 } });
            Console.WriteLine("Last operation Rus consumed "+ result.ToString());
        }

        private static async Task GenerateSampleDocuments()
        {
            string JSONstr = File.ReadAllText("SampleDocumentToInsert.txt");
            JArray ObjModel = (JArray)JsonConvert.DeserializeObject(JSONstr);
            for (int i = 0; i < DocsCount; i++)
            {
                DocModel dm = BsonSerializer.Deserialize<DocModel>(ObjModel[0].ToString());
                dm.Id = Guid.NewGuid().ToString();
                sampleDocuments.Add(dm);
            }

        }

        private static async Task InsertAllDocuments()
        {
            Console.WriteLine("Inserting: "+DocsCount +" ...");
            Console.WriteLine("Batch size: " + WriteBatchSize + " ...");
            var batches = DocsCount / WriteBatchSize;
            for (int j = 0; j < batches; j++)
            {
                var starTime = DateTime.UtcNow;
                var tasks = new List<Task>();
                var start = WriteBatchSize * j;
                var end = start + WriteBatchSize;
                for (int i = start; i < WriteBatchSize; i++)
                {
                    tasks.Add(InsertDocument(sampleDocuments[i]));
                }

                await Task.WhenAll(tasks).ConfigureAwait(false);
                var endTime = DateTime.UtcNow;
                var totalTime = endTime.Subtract(starTime);
                Console.WriteLine(
                    "Total time taken to this run this Insertion operation: min -{0} - sec {1}- milliseconds- {2} - Batch Id: {3}",
                    totalTime.Minutes,
                    totalTime.Seconds,
                    totalTime.Milliseconds,
                    j);
            }
        }

        private static async Task ReadAllDocuments()
        {
            Console.WriteLine("Reading: " + DocsCount + " ...");
            var starTime = DateTime.UtcNow;
            var tasks = new List<Task>();
            for (int i = 0; i < DocsCount; i++)
            {
                tasks.Add(ReadDocument(sampleDocuments[i]));
            }
           
            await Task.WhenAll(tasks).ConfigureAwait(false);
            var endTime = DateTime.UtcNow;
            var totalTime = endTime.Subtract(starTime);
            Console.WriteLine(
                "Total time taken to this run this read operation: min -{0} - sec {1}- milliseconds- {2}",
                totalTime.Minutes,
                totalTime.Seconds,
                totalTime.Milliseconds);
        }

        private static async Task ReadDocument(DocModel doc)
        {
            var builder = Builders<DocModel>.Filter;
            FilterDefinition<DocModel> filter = builder.Eq("_id", doc.Id);
            bool isSucceed = false;
            for (int i = 0; i < ReadRetries; i++)
            {
                try
                {
                    await docStoreCollection.Find(filter).ForEachAsync<DocModel>(document => receivedDocuments.Add(document));

                    //Operation succeed just break the loop
                    isSucceed = true;
                    break;
                }
                catch (Exception ex)
                {

                    if (!IsThrottled(ex))
                    {
                        Console.WriteLine("ERROR: With collection {0}", ex.ToString());
                        throw;
                    }
                    else
                    {
                        // Thread will wait in between 1.5 secs and 3 secs.
                        System.Threading.Thread.Sleep(new Random().Next(MinWait, MaxWait));
                    }
                }
            }
            if (!isSucceed)
            {
                ReadFailedDocs.Add(doc);
            }

        }

        private static async Task InsertDocument(DocModel doc)
        {
            bool isSucceed = false;
            for (int i = 0; i < InsertRetries; i++)
            {
                try
                {
                    await docStoreCollection.InsertOneAsync(doc);

                    isSucceed = true;
                    //Operation succeed just break the loop
                    break;
                }
                catch (Exception ex)
                {
                    
                    if (!IsThrottled(ex))
                    {
                        Console.WriteLine("ERROR: With collection {0}", ex.ToString());
                        throw;
                    }
                    else
                    {
                        // Thread will wait in between 1.5 secs and 3 secs.
                        System.Threading.Thread.Sleep(new Random().Next(MinWait,MaxWait));
                    }
                }
            }

            if (!isSucceed)
            {
                InsertFailedDocs.Add(doc);
            }

        }

        private static bool IsThrottled(Exception ex)
        {
            return ex.Message.ToLower().Contains("Request rate is large".ToLower());
        }
    }
}
