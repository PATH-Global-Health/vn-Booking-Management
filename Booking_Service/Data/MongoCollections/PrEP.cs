using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoCollections
{

    public class PrEP
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
        public PrEP_Infomation PrEP_Infomation { get; set; }
        public List<TX_ML>  TX_ML { get; set; }

    }


    public class PrEP_Infomation
    {
        public DateTime StartDate { get; set; }
        public string Code { get; set; }
    }

    public class TX_ML
    {
        public DateTime ReportDate { get; set; }
        public string TimingLate { get; set; }
        public string Status { get; set; }
        public int IsLate { get; set; }
    }
}
