﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.DataAccess;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;

namespace Services.Core
{
    public interface ITestingHistoryService
    {
        Task<ResultModel> Add(TestingHistoryCreateModel model);
        Task<ResultModel> CreateLayTest(LayTestCreateModel model);

        Task<ResultModel> GetLayTest(string employeeId,string employeeName,string customer, Guid? customerId = null);
        Task<ResultModel> GetLayTestById(Guid laytestId);



    }
    public class TestingHistoryService: ITestingHistoryService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public TestingHistoryService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ResultModel> Add(TestingHistoryCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<TestingHistoryCreateModel, TestingHistory>(model);
                await _context.TestingHistory.InsertOneAsync(data);
                result.Data = model;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> CreateLayTest(LayTestCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<LayTestCreateModel, TestingHistory>(model);
                await _context.TestingHistory.InsertOneAsync(data);
                result.Data = model;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> GetLayTest(string employeeId, string employeeName, string customerName, Guid? customerId = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<TestingHistory>.Filter.Empty;

                if (!string.IsNullOrEmpty(employeeId))
                {
                    var employeeIdFilter =
                        Builders<TestingHistory>.Filter.Eq(x => x.CDO_Employee.EmployeeId , employeeId);

                    basefilter = basefilter & employeeIdFilter;
                }

                if (!string.IsNullOrEmpty(employeeName))
                {
                    var employeeNameFilter =
                        Builders<TestingHistory>.Filter.Eq(x => x.CDO_Employee.Name, employeeName);

                    basefilter = basefilter & employeeNameFilter;
                }

                if (!string.IsNullOrEmpty(customerName))
                {
                    var customerNameFilter =
                        Builders<TestingHistory>.Filter.Eq(x => x.Customer.Fullname, customerName);

                    basefilter = basefilter & customerNameFilter;
                }

                if (customerId.HasValue)
                {
                    var customerIdFilter =
                        Builders<TestingHistory>.Filter.Eq(x => x.Customer.Id, customerId);

                    basefilter = basefilter & customerIdFilter;
                }


                var rs = await _context.TestingHistory.FindAsync(basefilter);
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<TestingHistory>, List<LayTestViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> GetLayTestById(Guid laytestId)
        {
            var result = new ResultModel();
            try
            {
                var entity = await _context.TestingHistory.FindAsync(x => x.Id == laytestId);
                var data = _mapper.Map<TestingHistory, LayTestViewModel>(entity.FirstOrDefault());
                result.Data = data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }






    }
}
