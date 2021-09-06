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

    public interface IPrEPService
    {
        Task<ResultModel> Add(PrEPCreateModel model);
    }

    public class PrEPService: IPrEPService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        public PrEPService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Add

        public async Task<ResultModel> Add(PrEPCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<PrEPCreateModel, PrEP>(model);
                await _context.PrEP.InsertOneAsync(data);
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
    }
}
