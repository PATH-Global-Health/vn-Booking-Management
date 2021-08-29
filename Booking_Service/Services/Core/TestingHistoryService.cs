using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Data.DataAccess;
using Data.MongoCollections;
using Data.ViewModels;

namespace Services.Core
{
    public interface ITestingHistoryService
    {
        Task<ResultModel> Add(TestingHistoryCreateModel model);
        Task<ResultModel> CreateLayTest(LayTestCreateModel model);

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


    }
}
