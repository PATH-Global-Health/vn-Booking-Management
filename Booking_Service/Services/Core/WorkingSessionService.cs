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
using Services.RabbitMQ;

namespace Services.Core
{
    public interface IWorkingSessionService
    {
        Task<ResultModel> CreateSession(WorkingSessionCreateModel model);
        Task<ResultModel> FilterByEmployee(string empId, Guid? customerId = null);
        Task<ResultModel> GetSessionByCustomerId(string empId, Guid customerId);
        ResultModel TestRabit(TicketEmployeeModel model);
    }

    public class WorkingSessionService : IWorkingSessionService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IProducerAddReferTicket _producer;

        public WorkingSessionService(ApplicationDbContext context, IMapper mapper, IProducerAddReferTicket producer)
        {
            _context = context;
            _mapper = mapper;
            _producer = producer;
        }

        public async Task<ResultModel> CreateSession(WorkingSessionCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                if (model.Session.TimeEnd <= model.Session.TimeStart)
                {
                    throw new Exception("TimeEnd must greater than TimeStart");
                }

                var data = _mapper.Map<WorkingSessionCreateModel, WorkingSession>(model);
                if (!model.SessionContent.IsConsulstation)
                {
                    switch (model.SessionContent.Type)
                    {
                        case SesstionType.LAY_TEST:
                        {
                            // add laytest to history and assign laytestId to resultTestingId
                            var layTest = _mapper.Map<WorkingSessionCreateModel, TestingHistory>(model);
                            layTest.Result = new Result
                            {
                                Type = TestingType.LAY_TEST,
                                ResultTesting = model.SessionContent.Result,
                                Code = model.SessionContent.Code
                            };
                            data.SessionContent.ResultTestingId = layTest.Id.ToString();
                            await _context.TestingHistory.InsertOneAsync(layTest);
                            break;
                        }
                        case SesstionType.RECENCY:
                        {
                            var recency = _mapper.Map<WorkingSessionCreateModel, TestingHistory>(model);
                            recency.Result = new Result
                            {
                                Type = TestingType.RECENCY,
                                ResultTesting = model.SessionContent.Result,
                            };
                            data.SessionContent.ResultTestingId = recency.Id.ToString();
                            await _context.TestingHistory.InsertOneAsync(recency);
                            break;
                        }
                        case SesstionType.PrEP:
                        {
                            var prEP = _mapper.Map<WorkingSessionCreateModel, PrEP>(model);
                            data.SessionContent.ResultTestingId = prEP.Id.ToString();
                            await _context.PrEP.InsertOneAsync(prEP);
                            break;
                        }
                        case SesstionType.ART:
                        {
                            var art = _mapper.Map<WorkingSessionCreateModel, ART>(model);
                            data.SessionContent.ResultTestingId = art.Id.ToString();
                            await _context.ART.InsertOneAsync(art);
                            break;
                        }
                    }
                }


                if (model.SessionContent.Type == SesstionType.ART ||
                    model.SessionContent.Type == SesstionType.PrEP ||
                    model.SessionContent.Type == SesstionType.RECENCY)
                {
                    ReferType refType = (ReferType) ((int) model.SessionContent.Type - 2);
                    var ticket = new TicketEmployeeModel
                    {
                        EmployeeId = model.CDO_Employee.EmployeeId,
                        FromUnitId = new Guid(model.Facility.FacilityId),
                        ToUnitId = new Guid(model.SessionContent.ToUnitId),
                        ProfileId = model.Customer.Id,
                        Type = refType
                    };

                    var resultAddReferTicket = JsonConvert.DeserializeObject<ResultModel>(SyncAddReferTicket(ticket));
                    if (!resultAddReferTicket.Succeed) throw new Exception("refer failed");
                }

                await _context.WorkingSession.InsertOneAsync(data);
                result.Data = data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null
                    ? e.InnerException.Message + "\n" + e.StackTrace
                    : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        private string SyncAddReferTicket(TicketEmployeeModel externalId)
        {
            // to json
            var message = JsonConvert.SerializeObject(externalId);
            //sync instance with MSSQL and api
            var response = _producer.Call(message, RabbitQueue.AddReferTicket); // call and wait for response
            return response;
        }

        public ResultModel TestRabit(TicketEmployeeModel model)
        {
            var result = new ResultModel();
            try
            {
                result.Data = SyncAddReferTicket(model);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null
                    ? e.InnerException.Message + "\n" + e.StackTrace
                    : e.Message + "\n" + e.StackTrace;
            }

            return result;

        }

        public async Task<ResultModel> FilterByEmployee(string empId,Guid? customerId=null)
        {
            var result = new ResultModel();
            try
            {
                var baseFilter =
                    Builders<WorkingSession>.Filter.Eq(x => x.CDO_Employee.EmployeeId, empId);

                //Filter

                if (customerId.HasValue)
                {
                    var customerFilter = Builders<WorkingSession>.Filter.Eq(x => x.Customer.Id, customerId);
                    baseFilter = baseFilter & customerFilter;
                }

                var rs = await _context.WorkingSession.FindAsync(baseFilter);
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<WorkingSession>, List<WorkingSessionViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null
                    ? e.InnerException.Message + "\n" + e.StackTrace
                    : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultModel> GetSessionByCustomerId(string empId, Guid customerId)
        {
            var result = new ResultModel();
            try
            {
                var session =
                    await _context.WorkingSession.FindAsync(x => x.Customer.Id == customerId && x.IsDelete == false);
                var data = session.FirstOrDefault();
                result.Data = _mapper.Map<WorkingSession, WorkingSessionViewModel>(data);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null
                    ? e.InnerException.Message + "\n" + e.StackTrace
                    : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }
    }
}
