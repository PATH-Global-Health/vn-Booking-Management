using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Data.ViewModels
{
    public class LayTestModel
    {
        [Required]
        public CustomerModel Customer { get; set; }
        [Required]
        public CDO_EmployeeModel CDO_Employee { get; set; }
        [Required]
        public ResultTestingModel Result { get; set; }
    }

    public class LayTestCreateModel : LayTestModel
    {
       
    }



}
