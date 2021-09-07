using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.ViewModels
{
    public class ARTModel
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
        public ART_InfomationModel ART_Infomation { get; set; }
        [Required]
        public List<TX_ML_Model> TX_ML { get; set; }
    }

    public class ARTCreateModel : ARTModel
    {

    }

    public class ARTViewModel : ARTModel
    {
        public Guid Id { get; set; }
        public bool IsDelete { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; }
    }

    public class ART_InfomationModel
    {
        public DateTime StartDate { get; set; }
        public string Code { get; set; }
    }


}
