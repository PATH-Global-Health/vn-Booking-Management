using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Data.Enums;

namespace Data.ViewModels
{
    class ValidationDataToDHealth
    {
    }

    public class LayTestValidationDHealth
    {

    }

    public class FacilityLayTestModel
    {
        [Required]
        [StringLength(4)]
        public string FacilityId { get; set; }
        public string Name { get; set; }
    }

    public class CustomerLayTestModel
    {
        public Guid Id { get; set; }
    }
    public class CDO_EmployeeLayTestModel
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
    }
    public class ResultTestingLayTestModel
    {
        public TestingType Type { get; set; }
        
        public double HIVPublicExaminationDate { get; set; }
        public string PublicExaminationOrder { get; set; }
        public int ExaminationForm { get; set; }
        public string ReceptionId { get; set; }
        public string ResultDate { get; set; } // Ngày có kết quả
        public string Code { get; set; }

    }
}
