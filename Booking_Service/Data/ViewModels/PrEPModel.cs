using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Data.MongoCollections;

namespace Data.ViewModels
{
    public class PrEPModel
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
        public PrEP_InfomationModel PrEP_Infomation { get; set; }
        [Required]
        public List<TX_ML_Model> TX_ML { get; set; }

    }

    public class PrEPCreateModel: PrEPModel
    {

    }

    public class PrEPViewModel : PrEPModel
    {
        public Guid Id { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; } 
        public DateTime DateUpdate { get; set; }
    }

    public class PrEP_InfomationModel
    {
        public DateTime StartDate { get; set; }
        public string Code { get; set; }
    }

    public class TX_ML_Model
    {
        public DateTime ReportDate { get; set; }
        public string TimingLate { get; set; }
        public string Status { get; set; }
        [Range(0,1)]
        public int IsLate { get; set; }
    }

    public class PrEPUpdateModel
    {
        public bool IsDelete { get; set; }
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
    }
}
