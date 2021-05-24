using Data.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.MongoCollections
{
    /// <summary>
    /// Medical Test V2
    /// </summary>
    public class Examination
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public Interval Interval { get; set; }
        public UnitV2 Unit { get; set; }
        public DoctorV2 Doctor { get; set; }
        public RoomV2 Room { get; set; }
        public ServiceV2 Service { get; set; }
        public CustomerV2 Customer { get; set; }
        public List<ContactV2> Contacts { get; set; }
        public BookingStatus Status { get; set; }
        public string Note { get; set; }
        public DateTime Date { get; set; }
        public BsonDocument Form { get; set; }
        public string BookedByUser { get; set; }
        public DateTime DateBooked { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime? ResultDate { get; set; }
        public string Result { get; set; }
        public ExitInformation ExitInformation { get; set; }
        public bool HasFile { get; set; }
    }

    public class ExitInformation
    {
        public string Destination { get; set; }
        public DateTime ExitingDate { get; set; }
        public DateTime EntryingDate { get; set; }
    }
}
