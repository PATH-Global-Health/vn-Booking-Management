using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Data.Enums;
using Data.MongoCollections;

namespace Data.ViewModels
{
    public class WorkingSessionModel
    {
        public Guid Id { get; set; } 
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; }
    }

    public class WorkingSessionCreateModel
    {
        [Required]
        public CustomerModel Customer { get; set; }
        [Required]
        public CDO_EmployeeModel CDO_Employee { get; set; }
        [Required]
        public FacilityModel Facility { get; set; }
        [Required]
        public SessionModel Session { get; set; }
        [Required]
        public SessionContentModel SessionContent { get; set; }

    }
    public class WorkingSessionViewModel:WorkingSessionModel
    {
        public CustomerModel Customer { get; set; }
        public CDO_EmployeeModel CDO_Employee { get; set; }
        public FacilityModel Facility { get; set; }
        public SessionModel Session { get; set; }
        public SessionContentModel SessionContent { get; set; }

    }


    public class SessionModel
    {
        public DateTime WorkingDate { get; set; }
        public DateTime TimeStart { get; set; }
        public DateTime TimeEnd { get; set; }
    }

    public class SessionContentModel
    {
        public bool IsConsulstation { get; set; }
        public SesstionType Type { get; set; }
        public string Note { get; set; }
        public string Code { get; set; }
        public string ResultTestingId { get; set; }
        public string Result { get; set; }
        public string FeedbackFromHospital { get; set; }
    }



}
