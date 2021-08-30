﻿using AutoMapper;
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



        }
    }
}
