using System;
using System.Collections.Generic;
using System.Linq;
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

    public interface IPrEPService
    {
        Task<ResultModel> Add(PrEPCreateModel model);
        Task<ResultModel> GetByCustomerId(Guid customerId, string unitId);
        Task<ResultModel> UpdatePrEP(Guid id, PrEPUpdateModel model);

    }

    public class PrEPService: IPrEPService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;
        private IProducerCheckExternalId _producer;

        public PrEPService(ApplicationDbContext context, IMapper mapper, IHttpClientFactory clientFactory, IProducerCheckExternalId producer)
        {
            _context = context;
            _mapper = mapper;
            _clientFactory = clientFactory;
            _producer = producer;
        }

        #region Add

        public async Task<ResultModel> Add(PrEPCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<PrEPCreateModel, PrEP>(model);
                await _context.PrEP.InsertOneAsync(data);
                if (!string.IsNullOrEmpty(model.Customer.ExternalId))
                {
                    var resultCheckExternalId = JsonConvert.DeserializeObject<ResultModel>(SyncExternalId(model.Customer.ExternalId));
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

        public ResultMessage SendDataToDHealth(PrEPCreateModel model)
        {
            ResultMessage rsMess = new ResultMessage();
            try
            {
                if (model.TX_ML.Count == 0 || model.TX_ML == null)
                {
                    rsMess = PushPrEP(model).Result;
                }
                else
                {
                    rsMess = PushTX_ML(model).Result;
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
        #endregion

        #region GetByCustomerId

        public async Task<ResultModel> GetByCustomerId(Guid customerId,string unitId)
        {
            var result = new ResultModel();
            try
            {
                var baseFilter = Builders<PrEP>.Filter.Eq(x => x.Customer.Id, customerId);

                if (!string.IsNullOrEmpty(unitId))
                {
                    baseFilter = baseFilter & Builders<PrEP>.Filter.Eq(x => x.Facility.FacilityId, unitId);
                }
                var rs = await _context.PrEP.FindAsync(baseFilter);
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<PrEP>, List<PrEPViewModel>>(list);
                result.Succeed = true;

            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        #endregion


        public async Task<ResultModel> UpdatePrEP(Guid id,PrEPUpdateModel model)
        {
            var result = new ResultModel();

            try
            {
                var filter = Builders<PrEP>.Filter.Eq(en => en.Id, id);
                var modelUpdated = _context.PrEP.Find(filter).FirstOrDefault();
                if (modelUpdated == null)
                {
                    throw new Exception("Can not find Id");
                }
                var update = Builders<PrEP>.Update.Set(mt => mt.IsDelete, model.IsDelete);

                if (!string.IsNullOrEmpty(model.Code))
                {
                    update = update.Set(en => en.PrEP_Infomation.Code, model.Code);
                }

                if (model.StartDate != null)
                {
                    update = update.Set(en => en.PrEP_Infomation.StartDate, model.StartDate);
                }

                await _context.PrEP.UpdateOneAsync(filter, update);
                var data = _context.PrEP.Find(filter).FirstOrDefault();

                #region Update WorkingSession
                var sessionFilter = Builders<WorkingSession>.Filter.Eq(en => en.SessionContent.ResultTestingId, id.ToString());
                if (sessionFilter != null)
                {
                    var updateSession = Builders<WorkingSession>.Update.Set(mt => mt.SessionContent.Result, "Processed");
                    updateSession = updateSession.Set(mt => mt.SessionContent.Code, model.Code);
                    await _context.WorkingSession.UpdateOneAsync(sessionFilter, updateSession);
                }
                #endregion
                result.Data = _mapper.Map<PrEP, PrEPViewModel>(data);
                result.Succeed = true;


            }
            catch (Exception e)
            {
                result.ResponseFailed = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }


        // Comunication


        #region ComunicationDhealth

        public async Task<ResultMessage> PushPrEP(PrEPCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var prEP = new PrEPPushModel
                {
                    donViDieuTriPrep = model.Facility.Name,
                    maSoDieuTriPrep = model.PrEP_Infomation.Code,
                    userId = model.Customer.ExternalId,
                    ngayBatDauDieuTriPrep = model.PrEP_Infomation.StartDate.
                        ToUniversalTime()
                        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalMilliseconds
                };
                var content = new StringContent(JsonConvert.SerializeObject(prEP), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.PrEP, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultMessage> PushTX_ML(PrEPCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var tx_ml = new TX_MLInfoModel
                {
                   userId = model.Customer.ExternalId,
                   type = Enum.GetName(typeof(TypeHTS_POS),1),
                   thongTinLoHenDieuTri = _mapper.Map<List<TX_ML_Model>,List<TX_MLPushModel>>(model.TX_ML)
                };
                var content = new StringContent(JsonConvert.SerializeObject(tx_ml), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.TX_ML, content);
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
