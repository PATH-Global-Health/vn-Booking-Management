using AutoMapper;
using Data.Constants;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using MongoDB.Driver;
using Newtonsoft.Json;
using Services.RabbitMQ;
using Services.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;

namespace Services.Core
{
    public interface IExaminationService
    {
        Task<ResultModel> Add(ExaminationCreateModel model, string username);
        Task<ResultModel> Get(Guid? hospitalId = null, int? status = null, DateTime? from = null, DateTime? to = null,Guid? doctorId=null);
        Task<ResultModel> Update(ExaminationUpdateModel model);

        Task<ResultModel> Delete(ExaminationDeleteModel model);

        Task<ResultModel> Rating(ExaminationRatingModel model);

        Task<ResultModel> UpdateResult(ExaminationUpdateResultModel model);
        Task<ResultModel> GetByCustomer(string username);
        Task<ResultModel> GetById(Guid id);
        Task<ResultModel> CreateResultForm(FormFileCreateModel model);
        Task<ResultModel> GetResultForm(Guid examId);
        Task<ResultModel> UpdateResultForm(FormFileUpdateModel model);
        Task<ResultModel> Statistic(Guid? unitId=null, DateTime? from = null, DateTime? to = null, Guid? doctorId = null);

        ResultModel TestRabit(IntervalSyncModel Model);


    }

    public class ExaminationService : IExaminationService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IProducerMQ _producer;
        private IProducerSetInterval _producerInterval;

        public ExaminationService(ApplicationDbContext context, IMapper mapper, IProducerMQ producer, IProducerSetInterval producerInterval)
        {
            _context = context;
            _mapper = mapper;
            _producer = producer;
            _producerInterval = producerInterval;
        }

        public ResultModel TestRabit(IntervalSyncModel model)
        {
            var result = new ResultModel();
            try
            {
                var message = JsonConvert.SerializeObject(model);
                var response = _producerInterval.Call(message, RabbitQueue.BookingIntervalSyncQueue); 
                result = JsonConvert.DeserializeObject<ResultModel>(response);
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }

            return result;

        }
        public async Task<ResultModel> Add(ExaminationCreateModel model, string username)
        {
            var result = new ResultModel();
            ResultModel syncResult = new ResultModel();
            //using (var session = _context.StartSession())
            {
                //session.StartTransaction();
                try
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        throw new Exception("Username is null or empty.");
                    }

                    // đồng bộ với Schedule
                    var syncModel = new IntervalSyncModel()
                    {
                        Id = model.Interval.Id,
                    };
                    var syncResponse = SyncInterval(syncModel);
                    syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                    if (!syncResult.Succeed)
                    {
                        throw new Exception(syncResult.ErrorMessage);
                    }


                    //---------------------------

                    //                    var filter = Builders<Examination>.Filter.Where(m => m.Interval.Id == model.Interval.Id
                    //                                                                         && (m.Status == BookingStatus.UNFINISHED || m.Status == BookingStatus.FINISHED));
                    //                    var existed = _context.Examinations.Find(filter).FirstOrDefault();
                    //                    if (existed != null)
                    //                    {
                    //                        throw new Exception("Khách hàng đã đặt giờ này.");
                    //                    }

                    //----------------------------
                    #region Concurrency check
                    //                    ReplaceOneResult replaceResult;
                    //                    var loadedInstance = _context.Interval.AsQueryable()
                    //                                                            .Where(doc => doc.Id == model.Interval.Id)
                    //                                                            .SingleOrDefault();
                    //                    if (loadedInstance == null)
                    //                    {
                    //                        loadedInstance = new Interval()
                    //                        {
                    //                            NumId = model.Interval.NumId,
                    //                            From = model.Interval.From,
                    //                            To = model.Interval.To,
                    //                            Id = model.Interval.Id,
                    //                            Version = 0
                    //                        };
                    //                        await _context.Interval.InsertOneAsync( loadedInstance);
                    //                    }
                    //                    var version = loadedInstance.Version;
                    //
                    //                    loadedInstance.Version++;
                    //                    replaceResult = await _context.Interval.ReplaceOneAsync( c => c.Id == model.Interval.Id
                    //                                                                    && c.Version == version, loadedInstance,
                    //                                                                    new ReplaceOptions { IsUpsert = false });
                    //                    if (replaceResult.ModifiedCount != 1)
                    //                    {
                    //                        throw new Exception("Giờ đã được đặt.");
                    //                    }
                    #endregion

                    Examination newModel = _mapper.Map<ExaminationCreateModel, Examination>(model);
                    newModel.BookedByUser = username;
                    newModel.Status = BookingStatus.UNFINISHED;
                    await _context.Examinations.InsertOneAsync( newModel);
                    
                   

                    result.Data = _mapper.Map<Examination, ExaminationViewModel>(newModel);
                    result.Succeed = true;
 //                   await session.CommitTransactionAsync();
                }
                catch (Exception e)
                {
 //                   await session.AbortTransactionAsync();
                    // Fail vì 1 lý do gì đó, nhưng schedule đã thay đổi trạng thái thành công -> thay đổi trạng thái của schedule về như cũ
//                    if (syncResult.Succeed)
//                    {
//                        // create an instanceSync model 
//                        var syncModel = new IntervalSyncModel()
//                        {
//                            Id = model.Interval.Id,
//                            IsAvailable = true,
//                        };
//                        SyncInterval(syncModel);
//                    }
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                    result.ErrorMessage += Environment.NewLine + "Unit Username: " + model.Unit.Username.GetUnitUsernameForExamination(); // Debug, ko cần thiết
                }
            }
            return result;
        }

        public async Task<ResultModel> Get(Guid? hospitalId, int? status = null, DateTime? from = null, DateTime? to = null,
            Guid? doctorId = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Examination>.Filter.Empty;

                if (hospitalId.HasValue)
                {
                    var hospitalIdFilter = Builders<Examination>.Filter.Eq(mt => mt.Unit.Id, hospitalId);
                    basefilter = basefilter & hospitalIdFilter;
                }            
                if (doctorId.HasValue)
                {
                    var doctorIdFilter = Builders<Examination>.Filter.Eq(mt => mt.Doctor.Id, doctorId);
                    basefilter = basefilter & doctorIdFilter;
                }

                if (status.HasValue)
                {
                    var statusFilter = Builders<Examination>.Filter.Eq(mt => mt.Status, (BookingStatus)status.Value);
                    basefilter = basefilter & statusFilter;
                }
                if (from.HasValue)
                {
                    var fromFilter = Builders<Examination>.Filter.Gte(mt => mt.Date, from.Value);
                    basefilter = basefilter & fromFilter;
                }
                if (to.HasValue)
                {
                    var toFilter = Builders<Examination>.Filter.Lte(mt => mt.Date, to.Value);
                    basefilter = basefilter & toFilter;
                }


                // return
                var rs = await _context.Examinations.FindAsync(basefilter);
               
                
                var list = await rs.ToListAsync();
                result.Data = _mapper.Map<List<Examination>, List<ExaminationViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> GetByCustomer(string username)
        {
            var result = new ResultModel();
            try
            {
                var rs = await _context.Examinations.FindAsync(_ => _.BookedByUser == username,new FindOptions<Examination, Examination>()
                {
                    Sort = Builders<Examination>.Sort.Descending(x=> x.Date)
                });
                var list = await rs.ToListAsync();

                result.Data = _mapper.Map<List<Examination>, List<ExaminationViewModel>>(list);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> GetById(Guid id)
        {
            var result = new ResultModel();
            try
            {
                var entity = await _context.Examinations.FindAsync(_ => _.Id == id);

                result.Data = _mapper.Map<Examination, ExaminationViewModel>(entity.FirstOrDefault());
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

       

        public async Task<ResultModel> Update(ExaminationUpdateModel model)
        {
            var result = new ResultModel();
 //           using (var session = _context.StartSession())
            {
 //               session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<Examination>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<Examination>.Update.Set(mt => mt.Status, (BookingStatus)model.Status);

                    // update note if any
                    if (!string.IsNullOrEmpty(model.Note))
                    {
                        update = update.Set(mt => mt.Note, model.Note);
                    }
                    if (!string.IsNullOrEmpty(model.Rate))
                    {
                        update = update.Set(mt => mt.Rate, model.Rate);
                    }
                    if (!string.IsNullOrEmpty(model.TypeRating))
                    {
                        update = update.Set(mt => mt.TypeRating, model.TypeRating);
                    }
                    if (!string.IsNullOrEmpty(model.Desc))
                    {
                        update = update.Set(mt => mt.Desc, model.Desc);
                    }

                    //------------------------------

                    if (!string.IsNullOrEmpty(model.ConsultingContent.Content))
                    {
                        update = update.Set(mt => mt.ConsultingContent.Content, model.ConsultingContent.Content);
                    }

                    if (!string.IsNullOrEmpty(model.ConsultingContent.Note))
                    {
                        update = update.Set(mt => mt.ConsultingContent.Note, model.ConsultingContent.Note);
                    }

                    if (!string.IsNullOrEmpty(model.ConsultingContent.Result))
                    {
                        update = update.Set(mt => mt.ConsultingContent.Result, model.ConsultingContent.Result);
                    }

                    if (!string.IsNullOrEmpty(model.ConsultingContent.Type))
                    {
                        update = update.Set(mt => mt.ConsultingContent.Type, model.ConsultingContent.Type);
                    }


                    if (!string.IsNullOrEmpty(model.TestingContent.Content))
                    {
                        update = update.Set(mt => mt.TestingContent.Content, model.TestingContent.Content);
                    }
                    if (!string.IsNullOrEmpty(model.TestingContent.Note))
                    {
                        update = update.Set(mt => mt.TestingContent.Note, model.TestingContent.Note);
                    }
                    if (!string.IsNullOrEmpty(model.TestingContent.Result))
                    {
                        update = update.Set(mt => mt.TestingContent.Result, model.TestingContent.Result);
                    }
                    

                    // execute statement
                    await _context.Examinations.UpdateOneAsync( filter, update);
                    var modelUpdated = _context.Examinations.Find(filter).FirstOrDefault();

//                    // try sync interval
//                    if (model.Status == (int)BookingStatus.CANCELED || model.Status == (int)BookingStatus.DOCTOR_CANCEL)
//                    {
//                        // create an instanceSync model 
//                        var syncModel = new IntervalSyncModel()
//                        {
//                            Id = modelUpdated.Interval.Id,
//                            IsAvailable = true,
//                        };
//                        // sync instance with schedule module
//                        var syncResponse = SyncInterval(syncModel);
//                        var syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
//
//                        if (!syncResult.Succeed)
//                        {
//                            // throw if fail to sync
//                            throw new Exception("Không thể đồng bộ với Shedule-Service: " + syncResult.ErrorMessage);
//                        }
//                    }

                    result.Data = _mapper.Map<Examination, ExaminationViewModel>(modelUpdated);
                    result.Succeed = true;
                    // commit transaction if status not canceled
 //                   session.CommitTransaction();
                }
                catch (Exception e)
                {
 //                   session.AbortTransaction();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }


        
        public async Task<ResultModel> Delete(ExaminationDeleteModel model)
        {
            var result = new ResultModel();
            //           using (var session = _context.StartSession())
            {
                //               session.StartTransaction();
                try
                {
                    // filter by Id
                    var filter = Builders<Examination>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<Examination>.Update.Set(mt => mt.Status, (BookingStatus)model.Status);

                    // update note if any

                    // execute statement
                    await _context.Examinations.UpdateOneAsync(filter, update);
                    var modelUpdated = _context.Examinations.Find(filter).FirstOrDefault();

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

                        if (modelUpdated.Unit.Username.Contains("hcdc."))
                        {
                            // create an exam cancel booking model
                            var cancelModel = new CancelBookingExamModel()
                            {
                                BookingExamId = modelUpdated.Id,
                                PersonId = modelUpdated.Customer.Id,
                                IntervalId = modelUpdated.Interval.Id,
                                Status = model.Status
                            };
                        }
                    }

                    result.Data = _mapper.Map<Examination, ExaminationViewModel>(modelUpdated);
                    result.Succeed = true;
                    // commit transaction if status not canceled
                    //                   session.CommitTransaction();
                }
                catch (Exception e)
                {
                    //                   session.AbortTransaction();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        public async Task<ResultModel> Rating(ExaminationRatingModel model)
        {
            var result = new ResultModel();
            //           using (var session = _context.StartSession())
            {
                //               session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<Examination>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<Examination>.Update.Set(mt => mt.Rate, model.Rate);

                    // update note if any
                    if (!string.IsNullOrEmpty(model.TypeRating))
                    {
                        update = update.Set(mt => mt.TypeRating, model.TypeRating);
                    }
                    if (!string.IsNullOrEmpty(model.Desc))
                    {
                        update = update.Set(mt => mt.Desc, model.Desc);
                    }

                    // execute statement
                    await _context.Examinations.UpdateOneAsync(filter, update);
                    var modelUpdated = _context.Examinations.Find(filter).FirstOrDefault();


                    result.Data = _mapper.Map<Examination, ExaminationViewModel>(modelUpdated);
                    result.Succeed = true;
                    // commit transaction if status not canceled
                    //                   session.CommitTransaction();
                }
                catch (Exception e)
                {
                    //                   session.AbortTransaction();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        private string SyncInterval(IntervalSyncModel syncModel)
        {
            // to json
            var message = JsonConvert.SerializeObject(syncModel);

            //sync instance with MSSQL and api
            var response = _producerInterval.Call(message, RabbitQueue.BookingIntervalSyncQueue); // call and wait for response

            return response;
        }

        public async Task<ResultModel> CreateResultForm(FormFileCreateModel model)
        {
            var result = new ResultModel();
  //          using (var session = _context.StartSession())
            {
 //               session.StartTransaction();
                try
                {
                    var fileExist = await _context.ResultForm.FindAsync(_ => _.ExamId == model.ExamId);
                    if (fileExist.FirstOrDefault() != null)
                    {
                        throw new Exception("Exam already has result form.");
                    }
                    // check file type
                    var file = model.FormData;
                    if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception($"Unsupported ContentType. Expected: application/pdf. Found: {file.ContentType}");
                    }
                    if (file.Length <= 0)
                    {
                        throw new Exception("File has no content.");
                    }

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        var form = new ResultForm()
                        {
                            Id = Guid.NewGuid(),
                            Data = fileBytes,
                            ExamId = model.ExamId
                        };
                        await _context.ResultForm.InsertOneAsync( form);

                        // filter by Id
                        var filter = Builders<Examination>.Filter.Eq(mt => mt.Id, model.ExamId);
                        // update status
                        var update = Builders<Examination>.Update.Set(mt => mt.HasFile, true);
                        update = update.Set(mt => mt.ResultDate, model.ResultDate);
                        update = update.Set(mt => mt.Result, model.Result);
                        update = update.Set(mt => mt.Status, BookingStatus.RESULTED);

                        // execute statement
                        await _context.Examinations.UpdateOneAsync( filter, update);
 //                       await session.CommitTransactionAsync();
                    }

                    result.Succeed = true;
                }
                catch (Exception e)
                {
   //                 await session.AbortTransactionAsync();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        public async Task<ResultModel> UpdateResultForm(FormFileUpdateModel model)
        {
            var result = new ResultModel();
//            using (var session = _context.StartSession())
            {
   //             session.StartTransaction();
                try
                {
                    bool isExist = false;
                    var fileExist = await _context.ResultForm.FindAsync(_ => _.ExamId == model.ExamId);
                    if (fileExist.FirstOrDefault() != null)
                    {
                        isExist = true;
                    }
                    // check file type
                    var file = model.FormData;
                    if (!string.Equals(file.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception($"Unsupported ContentType. Expected: application/pdf. Found: {file.ContentType}");
                    }
                    if (file.Length <= 0)
                    {
                        throw new Exception("File has no content.");
                    }

                    using (var ms = new MemoryStream())
                    {
                        file.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        var form = new ResultForm()
                        {
                            Id = Guid.NewGuid(),
                            Data = fileBytes,
                            ExamId = model.ExamId
                        };
                        if (isExist)
                        {
                            // filter by Id
                            var filter = Builders<ResultForm>.Filter.Eq(mt => mt.ExamId, model.ExamId);
                            // update status
                            var update = Builders<ResultForm>.Update.Set(mt => mt.Data, fileBytes);
                            await _context.ResultForm.UpdateOneAsync( filter, update);
                        }
                        else
                        {
                            await _context.ResultForm.InsertOneAsync( form);
                        }
                    }

                   // await session.CommitTransactionAsync();
                    result.Succeed = true;
                }
                catch (Exception e)
                {
                    //await session.AbortTransactionAsync();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        public async Task<ResultModel> GetResultForm(Guid examId)
        {
            var result = new ResultModel();
            try
            {
                var fileExist = await _context.ResultForm.FindAsync(_ => _.ExamId == examId);
                var file = fileExist.FirstOrDefault();
                if (file == null)
                {
                    throw new Exception("File not found.");
                }

                result.Data = file.Data;
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> UpdateResult(ExaminationUpdateResultModel model)
        {
            var result = new ResultModel();
            try
            {
                // filter by Id
                var filter = Builders<Examination>.Filter.Eq(mt => mt.Id, model.Id);
                // update result
                var update = Builders<Examination>.Update.Set(mt => mt.Result, model.Result);
                // update result date
                update = update.Set(mt => mt.ResultDate, model.ResultDate);
                //
                update = update.Set(mt => mt.Status, BookingStatus.RESULTED);

                // execute statement
                await _context.Examinations.UpdateOneAsync(filter, update);
                var modelUpdated = _context.Examinations.Find(filter).FirstOrDefault();

                result.Data = _mapper.Map<Examination, ExaminationViewModel>(modelUpdated);
                result.Succeed = true;
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }

        public async Task<ResultModel> Statistic(Guid? unitId = null, DateTime? from = null, DateTime? to = null, Guid? doctorId = null)
        {
            var result = new ResultModel();
            try
            {
                var basefilter = Builders<Examination>.Filter.Empty;
                if (unitId.HasValue)
                {
                    basefilter = basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId);
                }

                if (doctorId.HasValue)
                {
                    basefilter = basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId);
                }

                if (from.HasValue)
                {
                    basefilter = basefilter & Builders<Examination>.Filter.Gte(mt => mt.Date, from.Value);
                }
                if (to.HasValue)
                {
                    basefilter = basefilter & Builders<Examination>.Filter.Lte(mt => mt.Date, to.Value);
                }




                var unfinishedFilter = unitId.HasValue ? 
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.UNFINISHED) :
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.UNFINISHED);


                var finishedFilter = unitId.HasValue ? 
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.FINISHED):
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.FINISHED);

                var canceledFilter = unitId.HasValue ?
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.CANCELED) :
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.CANCELED);

                var notDoingFilter = unitId.HasValue ?
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.NOT_DOING) :
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.NOT_DOING);

                var docCancelFilter = unitId.HasValue ?
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.DOCTOR_CANCEL) :
                     basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.DOCTOR_CANCEL);

                var resultedFilter = unitId.HasValue ?
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Unit.Id == unitId && _.Status == BookingStatus.RESULTED):
                    basefilter & Builders<Examination>.Filter.Where(_ => _.Doctor.Id == doctorId && _.Status == BookingStatus.RESULTED);
                // tasks
                var totalTask = _context.Examinations.CountDocumentsAsync(basefilter);
                var unfinishedTask = _context.Examinations.CountDocumentsAsync(unfinishedFilter);
                var finishedTask = _context.Examinations.CountDocumentsAsync(finishedFilter);
                var canceledTask = _context.Examinations.CountDocumentsAsync(canceledFilter);
                var notDoingTask = _context.Examinations.CountDocumentsAsync(notDoingFilter);
                var docCancelTask = _context.Examinations.CountDocumentsAsync(docCancelFilter);
                var resultedTask = _context.Examinations.CountDocumentsAsync(resultedFilter);
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
