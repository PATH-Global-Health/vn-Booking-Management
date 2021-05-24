using Data.Enums;
using Data.MongoCollections;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class BookingExamModel
    {
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Gender { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string IC { get; set; }
        public string UnitUsername { get; set; }
        public int? InstanceId { get; set; }
        public DateTime? InstanceTime { get; set; }
        public Guid? MedicalTestId { get; set; }
    }

    public class CancelBookingExamModel
    {
        public Guid? MedicalTestId { get; set; }
        public int PersonId { get; set; }
        public int? InstanceId { get; set; }
        public int Status { get; set; }
    }

    public class BookingExamModelV2
    {
        public Guid PersonId { get; set; }
        public string PersonName { get; set; }
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
        //
        public string UnitUsername { get; set; }
        public Guid? IntervalId { get; set; }
        public string IntervalFrom { get; set; }
        public string IntervalTo { get; set; }
        public DateTime? BookingDate { get; set; }
        public Guid? BookingExamId { get; set; }
    }

    public class CancelBookingExamModelV2
    {
        public Guid? BookingExamId { get; set; }
        public Guid PersonId { get; set; }
        public Guid? IntervalId { get; set; }
        public int Status { get; set; }
    }
}
