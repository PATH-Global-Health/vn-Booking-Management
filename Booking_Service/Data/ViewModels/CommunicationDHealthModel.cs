using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using Data.Enums;

namespace Data.ViewModels
{
    class CommunicationDHealthModel
    {
    }

    public class ViralLoadPushModel
    {
        public string userId { get; set; }
        public double testDateTLVR { get; set; }
        public string testResultTLVR { get; set; }
    }
    public class CD4PushModel
    {
        public string userId { get; set; }
        public double testDateCD4 { get; set; }
        public string testResultCD4 { get; set; }
    }
    public class RecencyPushModel
    {
        public string userId { get; set; }
        public double testDateRecency { get; set; }
        public string testResultRecency { get; set; }
    }

    public class HTS_POSPushModel
    {
        public string userId { get; set; }
        public double ngayLayMauXetNghiemKhangDinhHIV { get; set; }
        public string ketQuaXetNghiemKhangDinh { get; set; }
        public string donViLayMauXetNghiemKhangDinh { get; set; }
        public string maXetNghiemKhangDinhHIV { get; set; }

    }

    public class ARTPushModel
    {
        public string userId { get; set; }
        public double ngayBatDauDieuTriHIV { get; set; }
        public string maSoDieuTriHIV { get; set; }
        public string donViDieuTriHIV { get; set; }

    }
    public class PrEPPushModel
    {
        public string userId { get; set; }
        public double ngayBatDauDieuTriPrep { get; set; }
        public string maSoDieuTriPrep { get; set; }
        public string donViDieuTriPrep { get; set; }

    }

    public class TX_MLPushModel
    {
        public long reportDate { get; set; }
        public string thoiDiemTreHen { get; set; }
        public string tinhTrangDieuTri { get; set; }
        public int LoHenDieuTri { get; set; }

    }

    public class TX_MLInfoModel
    {
        public string userId { get; set; }
        public object type { get; set; }
        [Required]
        public List<TX_MLPushModel> thongTinLoHenDieuTri { get; set; }

    }


}
