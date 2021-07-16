using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.ViewModels
{
    public class ExaminationCreateModel
    {
        [Required]
        public IntervalModel Interval { get; set; }
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
        public string Note { get; set; }    
        public Object Form { get; set; }
        public DateTime Date { get; set; }
        public string BookedByUser { get; set; }
        [Required]
        public ExitInformationModel ExitInformation { get; set; }
    }

    public class ExaminationViewModel : ExaminationCreateModel
    {
        public Guid Id { get; set; }
        public int Status { get; set; }
        public DateTime DateBooked { get; set; }
        public DateTime? ResultDate { get; set; }
        public string Result { get; set; }
        public bool HasFile { get; set; }
    }

    public class ExaminationUpdateModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public int Status { get; set; }
        public string Note { get; set; }

        public string Rate { get; set; }
    }

    public class ExaminationUpdateResultModel
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public DateTime? ResultDate { get; set; }
        public string Result { get; set; }
    }

    public class ExitInformationModel
    {
        [Required]
        public string Destination { get; set; }
        [Required]
        public DateTime ExitingDate { get; set; }
        [Required]
        public DateTime EntryingDate { get; set; }
    }
}
