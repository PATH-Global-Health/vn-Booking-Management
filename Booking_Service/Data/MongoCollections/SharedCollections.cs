using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MongoCollections
{
    public class Interval
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        // Stt
        public int NumId { get; set; }
        public int Version { get; set; }
    }

    public class UnitV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
    }

    public class DoctorV2
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
    }

    public class RoomV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ServiceV2
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ServiceType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerV2
    {
        public Guid Id { get; set; }
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
        public string Nation { get; set; }
        public string PassportNumber { get; set; }
        public string VaccinationCode { get; set; }
    }

    public class ContactV2
    {
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ResultForm
    {
        public Guid Id { get; set; }
        public Guid ExamId { get; set; }
        public byte[] Data { get; set; }
    }
}
