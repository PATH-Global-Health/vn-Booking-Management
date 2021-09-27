using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Data.Enums;

namespace Data.ViewModels
{
    public class ValidationDataToDHealth
    {
        [Required]
        public FacilityLayTestModel Facility { get; set; }
        [Required]
        public CustomerLayTestModel Customer { get; set; }
        [Required]
        public CDO_EmployeeLayTestModel CDO_Employee { get; set; }
        [Required]
        public ResultTestingLayTestModel Result { get; set; }
    }

    public class LayTestValidationDHealth
    {
        [Required]
        public FacilityLayTestModel Facility { get; set; }
        [Required]
        public CustomerLayTestModel Customer { get; set; }
        [Required]
        public CDO_EmployeeLayTestModel CDO_Employee { get; set; }
        [Required]
        public ResultTestingLayTestModel Result { get; set; }

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
        [Required]
        public Guid Id { get; set; }
        public string ExternalId { get; set; }
    }
    public class CDO_EmployeeLayTestModel
    {
        public string EmployeeId { get; set; }
        public string Name { get; set; }
    }
    public class ResultTestingLayTestModel
    {
        public TestingType Type { get; set; }
        [Required]
        [Range(1,Double.MaxValue)]
        public double HIVPublicExaminationDate { get; set; }
        [Required]
        public string PublicExaminationOrder { get; set; }
        [Required]
        [Range(0,2)]
        public int ExaminationForm { get; set; }
        [Required]
        [StringLength(9)]
        public string ReceptionId { get; set; }
        public string ResultDate { get; set; } // Ngày có kết quả
        [Required]
        public string Code { get; set; }

    }
}
