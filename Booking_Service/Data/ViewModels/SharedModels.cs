using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class IntervalModel
    {
        public Guid Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        // Stt
        public int NumId { get; set; }
    }

    public class UnitModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
    }

    public class DoctorModel
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
    }

    public class RoomModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ServiceModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class ServiceTypeModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerModel
    {
        public Guid Id { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
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
        public string VaccinationCode { get; set; }
        public string ExternalId { get; set; }

    }

    public class ContactModel
    {
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
    }

    public class IntervalSyncModel
    {
        public Guid Id { get; set; }
        public bool IsAvailable { get; set; }
    }

}
