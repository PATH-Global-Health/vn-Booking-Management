using System;
using System.Collections.Generic;
using System.Text;
using Data.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoCollections
{
    public class WorkingSession
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; } 
        public DateTime DateUpdate { get; set; }
        public Customer Customer { get; set; }
        public CDO_Employee CDO_Employee { get; set; }
        public Facility Facility { get; set; }
        public Session Session { get; set; }
        public SessionContent SessionContent { get; set; }
    }

    public class Session
    {

        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
    }

    public class SessionContent
    {
        public bool IsConsulstation { get; set; }
        public SesstionType Type { get; set; }
        public string Note { get; set; }
        public string Code { get; set; }
        public string ResultTestingId { get; set; }
        public string Result { get; set; }
        public string FeedbackFromHospital { get; set; }
    }


}
