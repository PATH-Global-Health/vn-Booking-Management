using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Data.ViewModels
{
    public class VaccinationCreateModel
    {
        [Required]
        public IntervalModel Interval { get; set; }
        [Required]
        public UnitV2Model Unit { get; set; }
        [Required]
        public DoctorV2Model Doctor { get; set; }
        [Required]
        public RoomV2Model Room { get; set; }
        [Required]
        public ServiceV2Model Service { get; set; }
        [Required]
        public ServiceTypeModel ServiceType { get; set; }
        [Required]
        public CustomerV2Model Customer { get; set; }
        public List<ContactV2Model> Contacts { get; set; }
        public string Note { get; set; }
        public Object Form { get; set; }
        public DateTime Date { get; set; }
        public InjectionObjectModel injectionObject { get; set; }
        public string BookedByUser { get; set; }
    }

    public class VaccinationViewModel : VaccinationCreateModel
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DateBooked { get; set; }
    }

    public class VaccinationUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public int Status { get; set; }
        public string Note { get; set; }
    }
}
