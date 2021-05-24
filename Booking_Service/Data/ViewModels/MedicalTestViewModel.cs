using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    public class InstanceModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class InstanceViewModel
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int TimeBooked { get; set; }
        public int Version { get; set; }
    }

    public class UnitModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Information { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
    }

    public class DoctorModel
    {
        public string Id { get; set; }
        public string Fullname { get; set; }
    }

    public class RoomModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class ServiceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class CustomerModel
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Gender { get; set; }
        public string ProvinceCode { get; set; }
        public string DistrictCode { get; set; }
        public string WardCode { get; set; }
        public string IC { get; set; }
    }

    public class ContactModel
    {
        public string Fullname { get; set; }
        public string Phone { get; set; }
        public string Relationship { get; set; }
    }

    public class MedicalTestCreateModel
    {
        [Required]
        public InstanceModel Instance { get; set; }
        [Required]
        public UnitModel Unit { get; set; }
        [Required]
        public DoctorModel Doctor { get; set; }
        [Required]
        public RoomModel Room { get; set; }
        [Required]
        public ServiceModel Service { get; set; }
        [Required]
        public CustomerModel Customer { get; set; }
        public List<ContactModel> Contacts { get; set; }
        [Required]
        public int Status { get; set; }
        public string Note { get; set; }
        public Object Form { get; set; }
        public string BookedByUser { get; set; }
    }

    public class MedicalTestViewModel : MedicalTestCreateModel
    {
        public Guid Id { get; set; }
    }

    public class MedicalTestUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public int Status { get; set; }
        public string Note { get; set; }
    }
}
