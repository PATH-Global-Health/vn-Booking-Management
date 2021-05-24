using AutoMapper;
using Data.Constants;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using Services.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services.Core
{
    public interface IVaccinationService
    {
        Task<ResultModel> Add(VaccinationCreateModel model, string username);
        ResultModel Get(Guid hospitalId, int? status = null, DateTime? from = null, DateTime? to = null);
        ResultModel Update(VaccinationUpdateModel model);
        ResultModel GetByCustomer(string username);
        ResultModel GetById(Guid id);
        Task<ResultModel> Statistic(Guid unitId, DateTime? from = null, DateTime? to = null);
    }

    public class VaccinationService : IVaccinationService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IProducerMQ _producer;

        public VaccinationService(ApplicationDbContext context, IMapper mapper, IProducerMQ producer)
        {
            _context = context;
            _mapper = mapper;
            _producer = producer;
        }

        public async Task<ResultModel> Add(VaccinationCreateModel model, string username)
        {
            var result = new ResultModel();
            ResultModel syncResult = new ResultModel();

            using (var session = _context.StartSession())
            {
                session.StartTransaction();
                try
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        throw new Exception("Username is null or empty.");
                    }
                    var filter = Builders<Vaccination>.Filter.Where(m => m.Interval.Id == model.Interval.Id
                                                                            && m.Customer.Id == model.Customer.Id
                                                                            && m.Status == BookingStatus.UNFINISHED);
                    var existed = _context.Vaccinations.Find(filter).FirstOrDefault();
                    if (existed != null)
                    {
                        throw new Exception("Lịch kín.");
                    }
                    #region Concurrency check
                    ReplaceOneResult replaceResult;
                    var loadedInstance = _context.Interval.AsQueryable()
                                                            .Where(doc => doc.Id == model.Interval.Id)
                                                            .SingleOrDefault();
                    if (loadedInstance == null)
                    {
                        loadedInstance = new Interval()
                        {
                            NumId = model.Interval.NumId,
                            From = model.Interval.From,
                            To = model.Interval.To,
                            Id = model.Interval.Id,
                            Version = 0
                        };
                        await _context.Interval.InsertOneAsync(session, loadedInstance);
                    }
                    var version = loadedInstance.Version;

                    loadedInstance.Version++;
                    replaceResult = await _context.Interval.ReplaceOneAsync(session, c => c.Id == model.Interval.Id
                                                                    && c.Version == version, loadedInstance,
                                                                    new ReplaceOptions { IsUpsert = false });
                    if (replaceResult.ModifiedCount != 1)
                    {
                        throw new Exception("Giờ đã được đặt.");
                    }
                    #endregion
                    Vaccination newModel = _mapper.Map<VaccinationCreateModel, Vaccination>(model);
                    newModel.BookedByUser = username;
                    newModel.Status = BookingStatus.UNFINISHED;
                    _context.Vaccinations.InsertOne(session, newModel);

                    // update available false
                    // create an instanceSync model 
                    var syncModel = new IntervalSyncModel()
                    {
                        Id = model.Interval.Id,
                        IsAvailable = false,
                    };
                    // syncing instance
                    var syncResponse = SyncInterval(syncModel);
                    syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                    if (!syncResult.Succeed)
                    {
                        throw new Exception(syncResult.ErrorMessage);
                    }

                    //
                    result.Data = _mapper.Map<Vaccination, VaccinationViewModel>(newModel);
                    result.Succeed = true;
                    session.CommitTransaction();
                }
                catch (Exception e)
                {
                    session.AbortTransactionAsync();
                    //
                    if (syncResult.Succeed)
                    {
                        // create an instanceSync model 
                        var syncModel = new IntervalSyncModel()
                        {
                            Id = model.Interval.Id,
                            IsAvailable = true,
                        };
                        SyncInterval(syncModel);
                    }
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }
        public ResultModel Get(Guid hospitalId, int? status = null, DateTime? from = null, DateTime? to = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Vaccination>.Filter.Empty;
                var hospitalIdFilter = Builders<Vaccination>.Filter.Eq(mt => mt.Unit.Id, hospitalId);
                basefilter = basefilter & hospitalIdFilter;

                // filter status
                if (status.HasValue)
                {
                    var statusFilter = Builders<Vaccination>.Filter.Eq(mt => mt.Status, (BookingStatus)status.Value);
                    basefilter = basefilter & statusFilter;
                }
                if (from.HasValue)
                {
                    var fromFilter = Builders<Vaccination>.Filter.Gte(mt => mt.Date, from.Value);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var toFilter = Builders<Vaccination>.Filter.Lte(mt => mt.Date, to.Value);
                    basefilter = basefilter & toFilter;
                }

                // return
                List<Vaccination> rs = _context.Vaccinations.Find(basefilter).ToList();

                result.Data = _mapper.Map<List<Vaccination>, List<VaccinationViewModel>>(rs);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }
        public ResultModel Update(VaccinationUpdateModel model)
        {
            var result = new ResultModel();
            using (var session = _context.StartSession())
            {
                session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<Vaccination>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<Vaccination>.Update.Set(mt => mt.Status, (BookingStatus)model.Status);

                    // update note if any
                    if (!string.IsNullOrEmpty(model.Note))
                    {
                        update = update.Set(mt => mt.Note, model.Note);
                    }

                    // execute statement
                    _context.Vaccinations.UpdateOneAsync(session, filter, update);
                    var modelUpdated = _context.Vaccinations.Find(filter).FirstOrDefault();

                    // try sync interval
                    if (model.Status == (int)BookingStatus.CANCELED || model.Status == (int)BookingStatus.DOCTOR_CANCEL)
                    {
                        // create an instanceSync model 
                        var syncModel = new IntervalSyncModel()
                        {
                            Id = modelUpdated.Interval.Id,
                            IsAvailable = true,
                        };

                        // sync instance with schedule module
                        var syncResponse = SyncInterval(syncModel);
                        var syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);

                        if (!syncResult.Succeed)
                        {
                            // throw if fail to sync
                            throw new Exception("Không thể đồng bộ với Shedule-Service: " + syncResult.ErrorMessage);
                        }
                    }

                    result.Data = _mapper.Map<Vaccination, VaccinationViewModel>(modelUpdated);
                    result.Succeed = true;
                    // commit transaction if status not canceled
                    session.CommitTransaction();
                }
                catch (Exception e)
                {
                    session.AbortTransaction();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        public ResultModel GetByCustomer(string username)
        {
            var result = new ResultModel();
            try
            {
                var list = _context.Vaccinations.Find(_ => _.BookedByUser == username).ToList();

                result.Data = _mapper.Map<List<Vaccination>, List<VaccinationViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public ResultModel GetById(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var entity = _context.Vaccinations.Find(_ => _.Id == id).SingleOrDefault();

                result.Data = _mapper.Map<Vaccination, VaccinationViewModel>(entity);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        private string SyncInterval(IntervalSyncModel syncModel)
        {
            // to json
            var message = JsonConvert.SerializeObject(syncModel);

            //sync instance with MSSQL and api
            var response = _producer.Call(message, RabbitQueue.IntervalSyncQueue); // call and wait for response

            return response;
        }

        public async Task<ResultModel> Statistic(Guid unitId, DateTime? from = null, DateTime? to = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId);
                if (from.HasValue)
                {
                    basefilter = basefilter & Builders<Vaccination>.Filter.Gte(mt => mt.Date, from.Value);
                }
                if (to.HasValue)
                {
                    basefilter = basefilter & Builders<Vaccination>.Filter.Lte(mt => mt.Date, to.Value);
                }
                var unfinishedFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.UNFINISHED);
                var finishedFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.FINISHED);
                var canceledFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.CANCELED);
                var notDoingFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.NOT_DOING);
                var docCancelFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.DOCTOR_CANCEL);
                var resultedFilter = basefilter & Builders<Vaccination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.RESULTED);
                // tasks
                var totalTask = _context.Vaccinations.CountDocumentsAsync(basefilter);
                var unfinishedTask = _context.Vaccinations.CountDocumentsAsync(unfinishedFilter);
                var finishedTask = _context.Vaccinations.CountDocumentsAsync(finishedFilter);
                var canceledTask = _context.Vaccinations.CountDocumentsAsync(canceledFilter);
                var notDoingTask = _context.Vaccinations.CountDocumentsAsync(notDoingFilter);
                var docCancelTask = _context.Vaccinations.CountDocumentsAsync(docCancelFilter);
                var resultedTask = _context.Vaccinations.CountDocumentsAsync(resultedFilter);
                // result
                var total = await totalTask;
                var unfinished = await unfinishedTask;
                var finished = await finishedTask;
                var canceled = await canceledTask;
                var notDoing = await notDoingTask;
                var docCancel = await docCancelTask;
                var resulted = await resultedTask;
                //
                result.Data = new { TOTAL = total, UNFINISHED = unfinished, FINISHED = finished, CANCELED_BY_CUSTOMER = canceled, NOT_DOING = notDoing, CANCELED = docCancel, RESULTED = resulted };
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }
    }
}
