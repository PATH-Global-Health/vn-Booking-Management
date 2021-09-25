using System;
using System.Collections.Generic;
using System.Net.Http;
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
    public interface IWorkingSessionService
    {
        Task<ResultModel> CreateSession(WorkingSessionCreateModel model);
        Task<ResultModel> FilterByEmployee(string empId);
    }
    public class WorkingSessionService: IWorkingSessionService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public WorkingSessionService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

                await _context.WorkingSession.InsertOneAsync(data);
                result.Data = data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> FilterByEmployee(string empId)
        {
            var result = new ResultModel();
            try
            {
                var baseFilter =
                    Builders<WorkingSession>.Filter.Eq(x =>  x.CDO_Employee.EmployeeId,empId);

                //Filter

                var rs = await _context.WorkingSession.FindAsync(baseFilter);
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<WorkingSession>, List<WorkingSessionViewModel>>(list);
                result.Succeed = true;


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
