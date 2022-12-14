using Data.MongoCollections;
using MongoDB.Driver;
using System.Linq;

namespace Data.DataAccess
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _db;
        private IMongoClient _mongoClient;

        public ApplicationDbContext(IMongoClient client, string databaseName)
        {
            _db = client.GetDatabase(databaseName);
            _mongoClient = client;
        }
        public IMongoCollection<Examination> Examinations => _db.GetCollection<Examination>("examinations");
        public IMongoCollection<Contact> Contacts => _db.GetCollection<Contact>("contactV2s");
        public IMongoCollection<Customer> Customers => _db.GetCollection<Customer>("customerV2s");
        public IMongoCollection<Doctor> Doctors => _db.GetCollection<Doctor>("doctorV2s");
        public IMongoCollection<Interval> Interval => _db.GetCollection<Interval>("intervals");
        public IMongoCollection<Room> Rooms => _db.GetCollection<Room>("roomV2s");
        public IMongoCollection<Service> Services => _db.GetCollection<Service>("serviceV2s");
        public IMongoCollection<Unit> Units => _db.GetCollection<Unit>("unitV2s");
        public IMongoCollection<ResultForm> ResultForm => _db.GetCollection<ResultForm>("resultForms");
        public IMongoCollection<TestingHistory> TestingHistory => _db.GetCollection<TestingHistory>("testingHistory");
        public IMongoCollection<PrEP> PrEP => _db.GetCollection<PrEP>("preEP");
        public IMongoCollection<ART> ART => _db.GetCollection<ART>("art");
        public IMongoCollection<WorkingSession> WorkingSession => _db.GetCollection<WorkingSession>("workingSession");


        public IClientSessionHandle StartSession()
        {
            var session = _mongoClient.StartSession();
            return session;
        }

        public void CreateCollectionsIfNotExists()
        {
            var collectionNames = _db.ListCollectionNames().ToList();
            if (!collectionNames.Any(name => name == "examinations"))
            {
                _db.CreateCollection("examinations");
            }
            if (!collectionNames.Any(name => name == "resultForms"))
            {
                _db.CreateCollection("resultForms");
            }
            if (!collectionNames.Any(name => name == "intervals"))
            {
                _db.CreateCollection("intervals");
            }
            if (!collectionNames.Any(name => name == "testingHistory"))
            {
                _db.CreateCollection("testingHistory");
            }

            if (!collectionNames.Any(name => name == "preEP"))
            {
                _db.CreateCollection("preEP");
            }

            if (!collectionNames.Any(name => name == "art"))
            {
                _db.CreateCollection("art");
            }
            if (!collectionNames.Any(name => name == "workingSession"))
            {
                _db.CreateCollection("workingSession");
            }
        }
    }
}
