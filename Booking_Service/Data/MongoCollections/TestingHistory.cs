using System;
using System.Collections.Generic;
using System.Text;
using Data.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace Data.MongoCollections
{
    public class TestingHistory
    {
        [BsonId()]
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.UtcNow.AddHours(7);
        public App App { get; set; }
        public Facility Facility { get; set; }
        public Customer Customer { get; set; }
        public CDO_Employee CDO_Employee { get; set; }
        public Result Result { get; set; }

    }

    public class App
    {
        public string AppId { get; set; }
        public string Name { get; set; }
    }

    public class Facility
    {
        public string FacilityId { get; set; }
        public string Name { get; set; }
    }

    public class CDO_Employee
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
    }

    public class Result
    {
         public TestingType Type { get; set; }
         public double HIVPublicExaminationDate { get; set; }
         public string PublicExaminationOrder { get; set; }
         public int ExaminationForm { get; set; }
         public string ReceptionId { get; set; }
         public double TakenDate { get; set; }  // Ngày lấy mẫu
         public string TestingDate { get; set; } //   Ngày làm xét nghiệm
         public string ResultDate { get; set; } // Ngày có kết quả
         public string ResultTesting { get; set; } // Kết quả cuối cùng ( null nếu type = LayTEST)
         public double ViralLoad { get; set; }   // Lượng Virus (null nếu type != LayTest)
         public string Code { get; set; } 

    }


}
