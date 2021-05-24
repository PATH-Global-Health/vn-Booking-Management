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

        public IMongoCollection<Contact> Contacts => _db.GetCollection<Contact>("contacts");
        public IMongoCollection<Customer> Customers => _db.GetCollection<Customer>("customers");
        public IMongoCollection<Doctor> Doctors => _db.GetCollection<Doctor>("doctors");
        public IMongoCollection<Instance> Instances => _db.GetCollection<Instance>("instances");
        public IMongoCollection<MedicalTest> MedicalTests => _db.GetCollection<MedicalTest>("medicalTests");
        public IMongoCollection<Room> Rooms => _db.GetCollection<Room>("rooms");
        public IMongoCollection<Service> Services => _db.GetCollection<Service>("services");
        public IMongoCollection<Unit> Units => _db.GetCollection<Unit>("units");
        // Version 2
        public IMongoCollection<Examination> Examinations => _db.GetCollection<Examination>("examinations");
        public IMongoCollection<Vaccination> Vaccinations => _db.GetCollection<Vaccination>("vaccinations");
        public IMongoCollection<ContactV2> ContactV2s => _db.GetCollection<ContactV2>("contactV2s");
        public IMongoCollection<CustomerV2> CustomerV2s => _db.GetCollection<CustomerV2>("customerV2s");
        public IMongoCollection<DoctorV2> DoctorV2s => _db.GetCollection<DoctorV2>("doctorV2s");
        public IMongoCollection<Interval> Interval => _db.GetCollection<Interval>("intervals");
        public IMongoCollection<RoomV2> RoomV2s => _db.GetCollection<RoomV2>("roomV2s");
        public IMongoCollection<ServiceV2> ServiceV2s => _db.GetCollection<ServiceV2>("serviceV2s");
        public IMongoCollection<UnitV2> UnitV2s => _db.GetCollection<UnitV2>("unitV2s");
        public IMongoCollection<ResultForm> ResultForm => _db.GetCollection<ResultForm>("resultForms");

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
            if (!collectionNames.Any(name => name == "vaccinations"))
            {
                _db.CreateCollection("vaccinations");
            }
            if (!collectionNames.Any(name => name == "resultForms"))
            {
                _db.CreateCollection("resultForms");
            }
            if (!collectionNames.Any(name => name == "intervals"))
            {
                _db.CreateCollection("intervals");
            }
        }
    }
}
