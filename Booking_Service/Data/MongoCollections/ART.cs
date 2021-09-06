using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoCollections
{
    public class ART
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow.AddHours(7);
        public DateTime DateUpdate { get; set; }
        public App App { get; set; }
        public Facility Facility { get; set; }
        public Customer Customer { get; set; }
        public CDO_Employee CDO_Employee { get; set; }
        public ART_Infomation ART_Infomation { get; set; }
        public List<TX_ML> TX_ML { get; set; }
    }

    public class ART_Infomation
    {
        public DateTime StartDate { get; set; }
        public string Code { get; set; }
    }

}
