using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.Constants;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Services.RabbitMQ;

namespace Services.Core
{
    public interface ITestingHistoryService
    {
        Task<ResultModel> Add(TestingHistoryCreateModel model);
        Task<ResultModel> GetById(Guid id);
        Task<ResultModel> CreateLayTest(LayTestCreateModel model);
        Task<ResultModel> GetLayTest(string employeeId,string employeeName,string customer, Guid? customerId = null, int? pageIndex = 0, int? pageSize =0);
        Task<ResultModel> GetLayTestCustomer(string emoployId ,string customer, Guid? customerId = null);
        Task<ResultModel> GetLayTestByCustomerId(string customerId,int? pageIndex=0,int? pageSize=0);
        Task<ResultModel> GetLayTestById(Guid laytestId);
        Task<ResultModel> UpdateLayTest(LayTestUpdateModel model);
    }
    public class TestingHistoryService: ITestingHistoryService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;
        private IProducerCheckExternalId _producer;


        public TestingHistoryService(ApplicationDbContext context, IMapper mapper, IHttpClientFactory clientFactory, IProducerCheckExternalId producer)
        {
            _context = context;
            _mapper = mapper;
            _clientFactory = clientFactory;
            _producer = producer;
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

                // Send data to DHealth if externalId != null
                if (!string.IsNullOrEmpty(model.Customer.ExternalId))
                {
                    var resultCheckExternalId = JsonConvert.DeserializeObject< ResultModel >(SyncExternalId(model.Customer.ExternalId));
                    if (!resultCheckExternalId.Succeed)
                    {
                        throw new Exception("Invalid ExternalId");
                    }
                    ResultMessage mess = SendDataToDHealth(model);
                    if (mess.IsSuccessStatus)
                    {
                        result.Data = mess.Response;
                        result.Succeed = true;
                    }
                    else
                    {
                        result.ResponseFailed = mess.Response;
                    }
                }
                else
                {
                    result.Data = data;
                    result.Succeed = true;
                }
            }
            catch (Exception e)
            {
                result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }


        public ResultMessage SendDataToDHealth(TestingHistoryCreateModel model)
        {
            ResultMessage rsMess = new ResultMessage();
            try
            {
                switch (model.Result.Type)
                {
                    case TestingType.VIRAL_LOAD:
                    {
                        rsMess = PushViralLoad(model).Result;
                        break;
                    }
                    case TestingType.CD4:
                    {
                        rsMess = PushCD4(model).Result;
                        break;
                    }
                    case TestingType.RECENCY:
                    {
                        rsMess = PushRecency(model).Result;
                        break;
                    }
                    case TestingType.HTS_POS:
                    {
                        rsMess = PushHTS_POS(model).Result;
                        break;
                    }
                    case TestingType.LAY_TEST:
                    {
                        rsMess.IsSuccessStatus = true;
                        rsMess.Response = "Successfuly";
                        break;
                    }
                    default:
                    {
                        rsMess.Response = "Invalid Type";
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                rsMess.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return rsMess;
        }

        private string SyncExternalId(string externalId)
        {
            // to json
            var message = JsonConvert.SerializeObject(externalId);
            //sync instance with MSSQL and api
            var response = _producer.Call(message, RabbitQueue.ExistExternalIDQueue); // call and wait for response
            return response;
        }


        public ResultModel TestRabit(string model)
        {
            var result = new ResultModel();
            try
            {
                result.Data = SyncExternalId(model);
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
        public async Task<ResultModel> GetLayTest(
            string employeeId, string employeeName, string customerName, Guid? customerId = null,int? pageIndex =0,int?pageSize = 0)
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


                var data = _context.TestingHistory.Find(basefilter);
                PagingModel paging = new PagingModel(pageIndex ?? 0, pageSize ?? 0, data.CountDocuments());

                var list = await data
                    .Skip((paging.PageIndex - 1) * paging.PageSize)
                    .Limit(paging.PageSize)
                    .ToListAsync();

                paging.Data = _mapper.Map<List<TestingHistory>, List<LayTestViewModel>>(list);


                //                var rs = await _context.TestingHistory.FindAsync(basefilter);
                //                var list = await rs.ToListAsync();
                result.Data = paging;
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

        #region GetLayTestByCustomer
        public async Task<ResultModel> GetLayTestByCustomerId(string customerId, int? pageIndex = 0, int? pageSize = 0)
        {
            var result = new ResultModel();
            try
            {
                

                Guid id = new Guid(customerId);
                var basefilter = Builders<TestingHistory>.Filter.Eq(x => x.Customer.Id, id);

                var data = _context.TestingHistory.Find(basefilter);
                PagingModel paging = new PagingModel(pageIndex ?? 0, pageSize ?? 0, data.CountDocuments());

                var list = await data
                    .Skip((paging.PageIndex - 1) * paging.PageSize)
                    .Limit(paging.PageSize)
                    .ToListAsync();
//
//                var rs = await _context.TestingHistory.FindAsync(basefilter);
//                var list = await rs.ToListAsync();

                paging.Data = _mapper.Map<List<TestingHistory>, List<LayTestViewModel>>(list);
                result.Data = paging;
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


                #region Update WorkingSession
                var sessionFilter = Builders<WorkingSession>.Filter.Eq(en => en.SessionContent.ResultTestingId, model.Id.ToString());
                if (sessionFilter != null)
                {
                    var updateSession = Builders<WorkingSession>.Update.Set(mt => mt.SessionContent.Result, model.ResultTesting);
                    await _context.WorkingSession.UpdateOneAsync(sessionFilter, updateSession);
                }
                #endregion

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

        // Comunication

        #region ComunicationDhealth

        public async Task<ResultMessage> PushViralLoad(TestingHistoryCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var viralLoad = new ViralLoadPushModel
                {
                    userId = model.Customer.ExternalId,
                    testDateTLVR = (long) model.Result.TakenDate,
                    testResultTLVR = model.Result.ViralLoad.ToString()
                };
                var content = new StringContent(JsonConvert.SerializeObject(viralLoad), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.VIRAL_LOAD, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultMessage> PushCD4(TestingHistoryCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var cd4 = new CD4PushModel
                {
                    userId = model.Customer.ExternalId,
                    testDateCD4= (long)model.Result.TakenDate,
                    testResultCD4 = model.Result.ResultTesting
                };
                var content = new StringContent(JsonConvert.SerializeObject(cd4), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.CD4, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultMessage> PushRecency(TestingHistoryCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var cd4 = new RecencyPushModel()
                {
                    userId = model.Customer.ExternalId,
                    testDateRecency = (long)model.Result.TakenDate,
                    testResultRecency = model.Result.ResultTesting
                };
                var content = new StringContent(JsonConvert.SerializeObject(cd4), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.RECENCY, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }


        public async Task<ResultMessage> PushHTS_POS(TestingHistoryCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var hts_pos = new HTS_POSPushModel()
                {
                    userId = model.Customer.ExternalId,
                    ngayLayMauXetNghiemKhangDinhHIV = DateTime.Parse(model.Result.TestingDate) 
                        .ToUniversalTime()
                        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalMilliseconds,
                    donViLayMauXetNghiemKhangDinh = model.Facility.Name,
                    ketQuaXetNghiemKhangDinh = model.Result.ResultTesting,
                    maXetNghiemKhangDinhHIV = model.Result.Code
                };
                var content = new StringContent(JsonConvert.SerializeObject(hts_pos), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.HTS_POST, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        #endregion


    }
}
