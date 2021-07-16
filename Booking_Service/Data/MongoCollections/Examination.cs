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
        public Unit Unit { get; set; }
        public Doctor Doctor { get; set; }
        public Room Room { get; set; }
        public Service Service { get; set; }
        public Customer Customer { get; set; }
        public List<Contact> Contacts { get; set; }
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
        public string Rate { get; set; }
    }

    public class ExitInformation
    {
        public string Destination { get; set; }
        public DateTime ExitingDate { get; set; }
        public DateTime EntryingDate { get; set; }
    }
}
