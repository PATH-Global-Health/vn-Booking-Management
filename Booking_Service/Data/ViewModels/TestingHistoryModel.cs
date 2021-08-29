using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Data.Enums;

namespace Data.ViewModels
{
    public class TestingHistoryModel
    {
        public AppModel App { get; set; }
        public FacilityModel Facility { get; set; }
        public CustomerModel Customer { get; set; }
        public CDO_EmployeeModel CDO_Employee { get; set; }
        public ResultTestingModel Result { get; set; }
    }

    public class TestingHistoryCreateModel
    {
        [Required]
        public AppModel App { get; set; }
        [Required]
        public FacilityModel Facility { get; set; }
        [Required]
        public CustomerModel Customer { get; set; }
        [Required]
        public CDO_EmployeeModel CDO_Employee { get; set; }
        [Required]
        public ResultTestingModel Result { get; set; }
    }

    public class AppModel
    {
        public string AppId { get; set; }
        public string Name { get; set; }
    }

    public class FacilityModel
    {
        public string FacilityId { get; set; }
        public string Name { get; set; }
    }

    public class CDO_EmployeeModel
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
    }

    public class ResultTestingModel
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
