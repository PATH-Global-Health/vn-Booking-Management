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
    public interface IARTService
    {
        Task<ResultModel> Add(ARTCreateModel model);
        Task<ResultModel> GetByCustomerId(Guid customerId);
    }
    public class ARTService : IARTService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private readonly IHttpClientFactory _clientFactory;
        private IProducerCheckExternalId _producer;

        public ARTService(ApplicationDbContext context, IMapper mapper, IHttpClientFactory clientFactory, IProducerCheckExternalId producer)
        {
            _context = context;
            _mapper = mapper;
            _clientFactory = clientFactory;
            _producer = producer;
        }

        #region Add

        public async Task<ResultModel> Add(ARTCreateModel model)
        {
            var result = new ResultModel();
            try
            {
                var data = _mapper.Map<ARTCreateModel, ART>(model);
                await _context.ART.InsertOneAsync(data);
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


        public ResultMessage SendDataToDHealth(ARTCreateModel model)
        {
            ResultMessage rsMess = new ResultMessage();
            try
            {
                if (model.TX_ML.Count == 0 || model.TX_ML == null)
                {
                    rsMess = PushART(model).Result;
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

        public async Task<ResultModel> GetByCustomerId(Guid customerId)
        {
            var result = new ResultModel();
            try
            {
                var baseFilter = Builders<ART>.Filter.Eq(x => x.Customer.Id, customerId);

                var rs = await _context.ART.FindAsync(baseFilter);
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<ART>, List<ARTViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        #endregion


        #region ComunicationDhealth

        public async Task<ResultMessage> PushART(ARTCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var art = new ARTPushModel()
                {
                    donViDieuTriHIV = model.Facility.Name,
                    maSoDieuTriHIV = model.ART_Infomation.Code,
                    userId = model.Customer.ExternalId,
                    ngayBatDauDieuTriHIV = model.ART_Infomation.StartDate.
                        ToUniversalTime()
                        .Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                        .TotalMilliseconds
                };
                var content = new StringContent(JsonConvert.SerializeObject(art), Encoding.UTF8, "application/json");
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync(UriCommunicationDhealth.ART, content);
                result.IsSuccessStatus = response.IsSuccessStatusCode;
                result.Response = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                result.Response = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;
        }

        public async Task<ResultMessage> PushTX_ML(ARTCreateModel model)
        {
            var result = new ResultMessage();
            try
            {
                var tx_ml = new TX_MLInfoModel
                {
                    userId = model.Customer.ExternalId,
                    type = Enum.GetName(typeof(TypeHTS_POS), 2),
                    thongTinLoHenDieuTri = _mapper.Map<List<TX_ML_Model>, List<TX_MLPushModel>>(model.TX_ML)
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
