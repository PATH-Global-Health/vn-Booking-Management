using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.ViewModels
{
    public class FormFileCreateModel
    {
        public DateTime? ResultDate { get; set; }
        public string Result { get; set; }
        public Guid ExamId { get; set; }
        public IFormFile FormData { get; set; }
    }

    public class FormFileUpdateModel
    {
        public Guid ExamId { get; set; }
        public IFormFile FormData { get; set; }
    }
}
