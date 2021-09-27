using System;
using AutoMapper;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using Newtonsoft.Json;

namespace Services.MappingProfile
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //
            CreateMap<Contact, ContactModel>().ReverseMap();
            CreateMap<Customer, CustomerModel>().ReverseMap();
            CreateMap<Doctor, DoctorModel>().ReverseMap();
            CreateMap<Interval, IntervalModel>().ReverseMap();
            CreateMap<Room, RoomModel>().ReverseMap();
            CreateMap<Service, ServiceModel>().ReverseMap();
            CreateMap<ServiceType, ServiceTypeModel>().ReverseMap();
            CreateMap<Unit, UnitModel>().ReverseMap();
            CreateMap<ExitInformation, ExitInformationModel>().ReverseMap();
            //----------------
            CreateMap<App, AppModel>().ReverseMap();
            CreateMap<Facility, FacilityModel>().ReverseMap();
            CreateMap<Result, ResultTestingModel>().ReverseMap();
            CreateMap<CDO_Employee, CDO_EmployeeModel>().ReverseMap();
            CreateMap<PrEP_Infomation, PrEP_InfomationModel>().ReverseMap();
            CreateMap<ART_Infomation, ART_InfomationModel>().ReverseMap();
            CreateMap<TX_ML, TX_ML_Model>().ReverseMap();
            //-------------------
            CreateMap<Session, SessionModel>().ReverseMap();
            CreateMap<SessionContent, SessionContentModel>().ReverseMap();



            //
            //CreateMap<FormFile, FormFileCreateModel>().ReverseMap();
            //

            CreateMap<ExaminationCreateModel, Examination>()
                .ForMember(m => m.Form, op => op.MapFrom(vm => vm.Form != null ? BsonDocument.Parse(vm.Form.ToString()) : null));
            CreateMap<Examination, ExaminationViewModel>()
                .ForMember(m => m.Form, op => op.MapFrom(vm => BsonSerializer.Deserialize<object>(vm.Form, null)));
            CreateMap<ExaminationCreateModel, BookingExamModel>()
                .ForMember(m => m.PersonId, op => op.MapFrom(vm => vm.Customer.Id))
                .ForMember(m => m.PersonName, op => op.MapFrom(vm => vm.Customer.Fullname))
                .ForMember(m => m.Address, op => op.MapFrom(vm => vm.Customer.Address))
                .ForMember(m => m.BirthDate, op => op.MapFrom(vm => vm.Customer.BirthDate))
                .ForMember(m => m.WardCode, op => op.MapFrom(vm => vm.Customer.WardCode))
                .ForMember(m => m.DistrictCode, op => op.MapFrom(vm => vm.Customer.DistrictCode))
                .ForMember(m => m.ProvinceCode, op => op.MapFrom(vm => vm.Customer.ProvinceCode))
                .ForMember(m => m.Gender, op => op.MapFrom(vm => vm.Customer.Gender))
                .ForMember(m => m.IC, op => op.MapFrom(vm => vm.Customer.IC))
                .ForMember(m => m.Phone, op => op.MapFrom(vm => vm.Customer.Phone))
                .ForMember(m => m.Email, op => op.MapFrom(vm => vm.Customer.Email))
                .ForMember(m => m.IntervalId, op => op.MapFrom(vm => vm.Interval.Id))
                .ForMember(m => m.IntervalFrom, op => op.MapFrom(vm => vm.Interval.From))
                .ForMember(m => m.IntervalTo, op => op.MapFrom(vm => vm.Interval.To))
                .ForMember(m => m.BookingDate, op => op.MapFrom(vm => vm.Date))
                .ForMember(m => m.PassportNumber, op => op.MapFrom(vm => vm.Customer.PassportNumber))
                .ForMember(m => m.Nation, op => op.MapFrom(vm => vm.Customer.Nation))
                ;

            CreateMap<TestingHistoryCreateModel, TestingHistory>();
            CreateMap<LayTestCreateModel, TestingHistory>()
                .ForMember(m=>m.App,e=>e.Ignore())
                .ForMember(m => m.Facility, e => e.Ignore());

            CreateMap<TestingHistory, LayTestViewModel>();
                

            CreateMap<TestingHistory, TestingHistoryViewModel>();

            CreateMap<PrEPCreateModel, PrEP>();
            CreateMap<PrEP, PrEPViewModel>();

            CreateMap<ARTCreateModel, ART>();
            CreateMap<ART, ARTViewModel>();


            //DHealth

            CreateMap<TX_ML_Model, TX_MLPushModel>()
                .ForMember(p=> p.reportDate,
                    m=>m.MapFrom(mf=>mf.ReportDate.ToUniversalTime()
                    .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                    .TotalMilliseconds))
                .ForMember(p => p.thoiDiemTreHen, m => m.MapFrom(mf => mf.TimingLate))
                .ForMember(p => p.tinhTrangDieuTri, m => m.MapFrom(mf => mf.Status))
                .ForMember(p => p.LoHenDieuTri, m => m.MapFrom(mf => mf.IsLate));

            // session
            CreateMap<WorkingSessionCreateModel, WorkingSession>();
            CreateMap<WorkingSession, WorkingSessionViewModel>();
            CreateMap<WorkingSessionCreateModel, TestingHistory>();
            CreateMap<WorkingSessionCreateModel, PrEP>();
            CreateMap<WorkingSessionCreateModel, ART>();
            CreateMap<ResultTestingValidationModel, ResultTestingModel>();




            //--------------------------------------- Validation
            CreateMap<FacilityLayTestModel, FacilityModel>().ReverseMap();
            CreateMap<ResultTestingLayTestModel, ResultTestingModel>().ReverseMap();
            CreateMap<CDO_EmployeeLayTestModel, CDO_EmployeeModel>().ReverseMap();
            CreateMap<CustomerLayTestModel, CustomerModel>().ReverseMap();
            CreateMap<LayTestValidationDHealth, TestingHistory>();
            CreateMap<LayTestValidationDHealth, TestingHistoryCreateModel>();
        }
    }
}
