using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;

namespace Services.Core
{
    public interface ITestingHistoryService
    {
        Task<ResultModel> Add(TestingHistoryCreateModel model);
        Task<ResultModel> GetById(Guid id);
        Task<ResultModel> CreateLayTest(LayTestCreateModel model);
        Task<ResultModel> GetLayTest(string employeeId,string employeeName,string customer, Guid? customerId = null);

        Task<ResultModel> GetLayTestCustomer(string emoployId ,string customer, Guid? customerId = null);
        Task<ResultModel> GetLayTestById(Guid laytestId);
        Task<ResultModel> UpdateLayTest(LayTestUpdateModel model);
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

        // TESTING_HISTORY

        #region Add
        public async Task<ResultModel> Add(TestingHistoryCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<TestingHistoryCreateModel, TestingHistory>(model);
                await _context.TestingHistory.InsertOneAsync(data);
                result.Data = data.Id;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }
        #endregion

        #region GetById
        public async Task<ResultModel> GetById(Guid testHistoryId)
        {
            var result = new ResultModel();

            try
            {
                var entity = await _context.TestingHistory.FindAsync(x => x.Id == testHistoryId);
                var data = _mapper.Map<TestingHistory, TestingHistoryViewModel>(entity.FirstOrDefault());
                result.Data = data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }


            return result;
        }
        #endregion

        // LAYTEST

        #region AddLayTest
        public async Task<ResultModel> CreateLayTest(LayTestCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                model.Result.Type = TestingType.LAY_TEST;

                if (!string.IsNullOrEmpty((model.Result.ResultTesting)))
                {
                    model.Result.ResultDate = DateTime.UtcNow.AddHours(7).ToString();
                }

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
        #endregion

        #region GetLayTest
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
        #endregion

        #region GetLayTestById
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
        #endregion

        #region GetLayTestCustomer

        public async Task<ResultModel> GetLayTestCustomer(string emoployId, string customer, Guid? customerId = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<TestingHistory>.Filter.Eq(x => x.CDO_Employee.EmployeeId,emoployId);

                if (!string.IsNullOrEmpty(customer))
                {
                    var customerNameFilter = Builders<TestingHistory>.Filter.Eq(x => x.Customer.Fullname, customer);
                    basefilter = basefilter & customerNameFilter;
                }

                if (customerId.HasValue)
                {
                    var customerIdFiler = Builders<TestingHistory>.Filter.Eq(x => x.Customer.Id, customerId);
                    basefilter = basefilter & customerIdFiler;
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
        #endregion

        #region UpdateLayTest
        public async Task<ResultModel> UpdateLayTest(LayTestUpdateModel model)
        {
            var result = new ResultModel();
            try
            {
                var filter = Builders<TestingHistory>.Filter.Eq(en => en.Id, model.Id);
                var update = Builders<TestingHistory>.Update.Set(mt => mt.IsDelete, model.IsDelete);

                if (!string.IsNullOrEmpty(model.ResultTesting))
                {
                    update = update.Set(en => en.Result.ResultTesting, model.ResultTesting);
                    update = update.Set(en => en.Result.ResultDate, DateTime.UtcNow.AddHours(7).ToString());
                }

                if (!string.IsNullOrEmpty(model.Code))
                {
                    update = update.Set(en => en.Result.Code, model.Code);
                    update = update.Set(en => en.Result.TakenDate, model.TakenDate);
                }


                update = update.Set(en => en.DateUpdate, DateTime.UtcNow.AddHours(7));

                await _context.TestingHistory.UpdateOneAsync(filter, update);
                var modelUpdated = _context.TestingHistory.Find(filter).FirstOrDefault();
                var data = _mapper.Map<TestingHistory, LayTestViewModel>(modelUpdated);
                result.Data = data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }
        #endregion


    }
}
