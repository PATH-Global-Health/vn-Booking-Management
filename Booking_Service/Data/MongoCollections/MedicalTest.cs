using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MongoCollections
{
    public class MedicalTest
    {
        [BsonId]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Instance Instance { get; set; }
        public Unit Unit { get; set; }
        public Doctor Doctor { get; set; }
        public Room Room { get; set; }
        public Service Service { get; set; }
        public Customer Customer { get; set; }
        public List<Contact> Contacts { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public BsonDocument Form { get; set; }
        //[BsonIgnoreIfNull]
        //public string Result { get; set; }
        //[BsonIgnoreIfNull]
        //public DateTime ResultDate { get; set; }
        //[BsonIgnoreIfNull]
        //public bool HasFormFile { get; set; }
        public string BookedByUser { get; set; }
    }

    public class Instance
    {
        [BsonId]
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int TimeBooked { get; set; }
        public int Version { get; set; }
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
    }

    public class Doctor
    {
        [BsonRequired]
        public string Id { get; set; }
        [BsonRequired]
        public string Fullname { get; set; }
    }

    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Service
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Gender { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string IC { get; set; }
        public string NationalCode { get; set; }
    }

    public class Contact
    {
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
    }
}
