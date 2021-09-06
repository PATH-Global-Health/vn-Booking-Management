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
    public interface IARTService
    {
        Task<ResultModel> Add(ARTCreateModel model);
    }
    public class ARTService : IARTService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;

        public ARTService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        #region Add

        public async Task<ResultModel> Add(ARTCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<ARTCreateModel, ART>(model);
                await _context.ART.InsertOneAsync(data);
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
