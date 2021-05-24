using AutoMapper;
using Data.Constants;
using Data.DataAccess;
using Data.Enums;
using Data.MongoCollections;
using Data.ViewModels;
using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using Services.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public interface IMedicalTestService
    {
        Task<MedicalTestViewModel> AddTimeBooked(MedicalTestCreateModel newModel, string username);
        Task<MedicalTestViewModel> UpdateTimeBooked(MedicalTestUpdateModel model);
        Task<MedicalTestViewModel> Add(MedicalTestCreateModel newModel, string username);
        List<MedicalTestViewModel> GetSchedules(int hospitalId, int? status = null, DateTime? from = null, DateTime? to = null);
        Task<MedicalTestViewModel> Update(MedicalTestUpdateModel model);
        Task<ResultModel> UpdateFromExamination(MedicalTestUpdateModel model);
        MedicalTestViewModel GetById(Guid id);
        List<MedicalTestViewModel> GetByCustomer(string username);
        List<InstanceViewModel> GetInstances(int? id);
    }

    public class MedicalTestService : IMedicalTestService
    {
        private ApplicationDbContext _context;
        private IMapper _mapper;
        private IProducerMQ _producer;

        public MedicalTestService(ApplicationDbContext context, IMapper mapper, IProducerMQ producer)
        {
            _context = context;
            _mapper = mapper;
            _producer = producer;
        }

        public async Task<MedicalTestViewModel> AddTimeBooked(MedicalTestCreateModel newCreateModel, string username)
        {
            bool isBookingExam = newCreateModel.Unit.Username.Contains("hcdc.");
            ResultModel examResult = new ResultModel();
            ResultModel syncResult = new ResultModel();

            using (var session = _context.StartSession())
            {
                session.StartTransaction();
                try
                {
                    var medicalTestFilter = Builders<MedicalTest>.Filter.Where(m => m.Instance.Id == newCreateModel.Instance.Id
                                                                            && m.Customer.Id == newCreateModel.Customer.Id
                                                                            && m.Status == (int)MedicalTestStatus.UNFINISHED);
                    var existed = _context.MedicalTests.Find(medicalTestFilter).FirstOrDefault();
                    if (existed != null)
                    {
                        throw new Exception("Khách hàng đã đặt giờ này.");
                    }
                    ReplaceOneResult replaceResult;
                    var loadedInstance = _context.Instances.AsQueryable()
                                                            .Where(doc => doc.Id == newCreateModel.Instance.Id)
                                                            .SingleOrDefault();
                    if (loadedInstance == null)
                    {
                        loadedInstance = new Instance();
                        loadedInstance.Id = newCreateModel.Instance.Id;
                        loadedInstance.DateTime = newCreateModel.Instance.DateTime;
                        loadedInstance.TimeBooked = 0;
                        loadedInstance.Version = 0;
                        await _context.Instances.InsertOneAsync(session, loadedInstance);
                    }
                    var version = loadedInstance.Version;

                    loadedInstance.Version++;
                    loadedInstance.TimeBooked++;
                    replaceResult = await _context.Instances.ReplaceOneAsync(session, c => c.Id == newCreateModel.Instance.Id
                                                                    && c.Version == version, loadedInstance,
                                                                    new ReplaceOptions { IsUpsert = false });
                    if (replaceResult.ModifiedCount != 1)
                    {
                        throw new Exception("Lỗi instance version.");
                    }

                    MedicalTest newModel = _mapper.Map<MedicalTestCreateModel, MedicalTest>(newCreateModel);
                    newModel.BookedByUser = username;
                    newModel.Status = 1;
                    await _context.MedicalTests.InsertOneAsync(newModel);

                    // booking if posible
                    if (isBookingExam)
                    {
                        var examinationUsername = getUnitUsernameForExamination(newCreateModel.Unit.Username);
                        var bookingModel = new BookingExamModel
                        {
                            PersonId = newCreateModel.Customer.Id,
                            PersonName = newCreateModel.Customer.Fullname,
                            Address = newCreateModel.Customer.Address,
                            BirthDate = newCreateModel.Customer.BirthDate,
                            DistrictCode = newCreateModel.Customer.DistrictCode,
                            Email = newCreateModel.Customer.Email,
                            Gender = newCreateModel.Customer.Gender,
                            IC = newCreateModel.Customer.IC,
                            Phone = newCreateModel.Customer.Phone,
                            ProvinceCode = newCreateModel.Customer.ProvinceCode,
                            WardCode = newCreateModel.Customer.WardCode,
                            UnitUsername = examinationUsername,
                            InstanceId = newCreateModel.Instance.Id,
                            InstanceTime = newCreateModel.Instance.DateTime,
                            MedicalTestId = newModel.Id
                        };
                        var examResponse = BookingExamination(bookingModel);
                        examResult = JsonConvert.DeserializeObject<ResultModel>(examResponse);
                    }
                    if (!examResult.Succeed)
                    {
                        throw new Exception(examResult.ErrorMessage);
                    }

                    // update available false
                    if (loadedInstance.TimeBooked == 6)
                    {
                        // create an instanceSync model 
                        var syncModel = new InstanceSyncModel()
                        {
                            Id = newCreateModel.Instance.Id,
                            IsAvailable = false,
                        };
                        // syncing instance
                        var syncResponse = SyncInstance(syncModel);
                        syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                        if (!syncResult.Succeed)
                        {
                            throw new Exception(syncResult.ErrorMessage);
                        }
                    }
                    await session.CommitTransactionAsync();
                    return _mapper.Map<MedicalTest, MedicalTestViewModel>(newModel);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    //
                    if (syncResult.Succeed)
                    {
                        // create an instanceSync model 
                        var syncModel = new InstanceSyncModel()
                        {
                            Id = newCreateModel.Instance.Id,
                            IsAvailable = true,
                        };
                        SyncInstance(syncModel);
                    }
                    if (isBookingExam && examResult.Succeed)
                    {
                        CancelBookingExamModel model = new CancelBookingExamModel()
                        {
                            InstanceId = newCreateModel.Instance.Id,
                            PersonId = newCreateModel.Customer.Id,
                        };
                        CancelBookingExamination(model);
                    }
                    throw;
                }
            }
        }

        public async Task<MedicalTestViewModel> UpdateTimeBooked(MedicalTestUpdateModel model)
        {
            using (var session = _context.StartSession())
            {
                session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<MedicalTest>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<MedicalTest>.Update.Set(mt => mt.Status, model.Status);
                    // update note if any
                    if (!string.IsNullOrEmpty(model.Note))
                    {
                        update = update.Set(mt => mt.Note, model.Note);
                    }

                    // execute statement
                    await _context.MedicalTests.UpdateOneAsync(session, filter, update);
                    var modelUpdated = _context.MedicalTests.FindAsync(filter).Result.FirstOrDefault();
                    modelUpdated.Status = model.Status;
                    // try sync instance
                    if (model.Status == (int)MedicalTestStatus.CANCELED || model.Status == (int)MedicalTestStatus.DOCTOR_CANCEL)
                    {
                        ReplaceOneResult replaceResult;
                        var loadedInstance = _context.Instances.AsQueryable()
                                                            .Where(doc => doc.Id == modelUpdated.Instance.Id)
                                                            .SingleOrDefault();
                        var version = loadedInstance.Version;
                        loadedInstance.Version++;
                        loadedInstance.TimeBooked--;
                        replaceResult = await _context.Instances.ReplaceOneAsync(session, c => c.Id == modelUpdated.Instance.Id
                                                                        && c.Version == version, loadedInstance,
                                                                        new ReplaceOptions { IsUpsert = false });
                        if (replaceResult.ModifiedCount != 1)
                        {
                            throw new Exception("Lỗi instance.");
                        }

                        // update available
                        if (loadedInstance.TimeBooked == 5)
                        {
                            // create an instanceSync model 
                            InstanceSyncModel syncModel = new InstanceSyncModel()
                            {
                                Id = modelUpdated.Instance.Id,
                                IsAvailable = true,
                            };
                            // sync instance with MSSQL and api
                            var syncResponse = SyncInstance(syncModel);
                            var syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                            if (!syncResult.Succeed)
                                throw new Exception("Không thể đồng bộ với MSSQL: " + syncResult.ErrorMessage);
                        }

                        if (modelUpdated.Unit.Username.Contains("hcdc."))
                        {
                            // create an exam cancel booking model
                            CancelBookingExamModel cancelModel = new CancelBookingExamModel()
                            {
                                MedicalTestId = modelUpdated.Id,
                                PersonId = modelUpdated.Customer.Id,
                                InstanceId = modelUpdated.Instance.Id,
                                Status = model.Status
                            };
                            
                            // Cancel booking
                            var examResponse = CancelBookingExamination(cancelModel);
                            var examResult = JsonConvert.DeserializeObject<ResultModel>(examResponse);
                            // after sync success

                            // throw if fail to cancel
                            if (!examResult.Succeed)
                                throw new Exception("Không thể hủy bên xét nghiệm: " + examResult.ErrorMessage);
                        }
                    }

                    // commit transaction if status not canceled
                    await session.CommitTransactionAsync();
                    return _mapper.Map<MedicalTest, MedicalTestViewModel>(modelUpdated);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }

        }

        public async Task<ResultModel> UpdateFromExamination(MedicalTestUpdateModel model)
        {
            var result = new ResultModel();
            using (var session = _context.StartSession())
            {
                session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<MedicalTest>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<MedicalTest>.Update.Set(mt => mt.Status, model.Status);
                    // update note if any
                    if (!string.IsNullOrEmpty(model.Note))
                    {
                        update = update.Set(mt => mt.Note, model.Note);
                    }

                    // execute statement
                    await _context.MedicalTests.UpdateOneAsync(session, filter, update);
                    var modelUpdated = _context.MedicalTests.FindAsync(filter).Result.FirstOrDefault();

                    // try sync instance
                    if (model.Status == (int)MedicalTestStatus.CANCELED || model.Status == (int)MedicalTestStatus.DOCTOR_CANCEL)
                    {
                        ReplaceOneResult replaceResult;
                        var loadedInstance = _context.Instances.AsQueryable()
                                                            .Where(doc => doc.Id == modelUpdated.Instance.Id)
                                                            .SingleOrDefault();
                        var version = loadedInstance.Version;
                        loadedInstance.Version++;
                        loadedInstance.TimeBooked--;
                        replaceResult = await _context.Instances.ReplaceOneAsync(session, c => c.Id == modelUpdated.Instance.Id
                                                                        && c.Version == version, loadedInstance,
                                                                        new ReplaceOptions { IsUpsert = false });
                        if (replaceResult.ModifiedCount != 1)
                        {
                            throw new Exception("Lỗi instance.");
                        }

                        // update available
                        if (loadedInstance.TimeBooked == 5)
                        {
                            // create an instanceSync model 
                            InstanceSyncModel syncModel = new InstanceSyncModel()
                            {
                                Id = modelUpdated.Instance.Id,
                                IsAvailable = true,
                            };
                            // sync instance with MSSQL and api
                            var syncResponse = SyncInstance(syncModel);
                            var syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                            if (!syncResult.Succeed)
                                throw new Exception("Không thể đồng bộ với MSSQL: " + syncResult.ErrorMessage);
                        }
                    }

                    // commit transaction if status not canceled
                    await session.CommitTransactionAsync();
                    result.Succeed = true;
                }
                catch (Exception e)
                {
                    await session.AbortTransactionAsync();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                }
            }
            return result;
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="newCreateModel"></param>
        /// <returns></returns>
        public async Task<MedicalTestViewModel> Add(MedicalTestCreateModel newCreateModel, string username)
        {
            bool isBookingExam = newCreateModel.Unit.Username.Contains("hcdc.");
            ResultModel syncResult = new ResultModel();
            ResultModel examResult = new ResultModel();
            // create an instanceSync model 
            InstanceSyncModel syncModel = new InstanceSyncModel()
            {
                Id = newCreateModel.Instance.Id,
                IsAvailable = false,
            };
            try
            {
                // syncing instance
                var syncResponse = SyncInstance(syncModel);
                syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);
                if (!syncResult.Succeed)
                {
                    throw new Exception(syncResult.ErrorMessage);
                }

                // booking if posible
                if (isBookingExam && syncResult.Succeed)
                {
                    var examinationUsername = getUnitUsernameForExamination(newCreateModel.Unit.Username);
                    var bookingModel = new BookingExamModel
                    {
                        PersonId = newCreateModel.Customer.Id,
                        PersonName = newCreateModel.Customer.Fullname,
                        Address = newCreateModel.Customer.Address,
                        BirthDate = newCreateModel.Customer.BirthDate,
                        DistrictCode = newCreateModel.Customer.DistrictCode,
                        Email = newCreateModel.Customer.Email,
                        Gender = newCreateModel.Customer.Gender,
                        IC = newCreateModel.Customer.IC,
                        Phone = newCreateModel.Customer.Phone,
                        ProvinceCode = newCreateModel.Customer.ProvinceCode,
                        WardCode = newCreateModel.Customer.WardCode,
                        UnitUsername = examinationUsername,
                        InstanceId = newCreateModel.Instance.Id,
                        InstanceTime = newCreateModel.Instance.DateTime
                    };
                    var examResponse = BookingExamination(bookingModel);
                    examResult = JsonConvert.DeserializeObject<ResultModel>(examResponse);
                }
                if (!examResult.Succeed)
                {
                    throw new Exception(examResult.ErrorMessage);
                }

                // after sync success
                if (examResult.Succeed && syncResult.Succeed)
                {
                    MedicalTest newModel = _mapper.Map<MedicalTestCreateModel, MedicalTest>(newCreateModel);
                    newModel.BookedByUser = username;
                    newModel.Status = 1;
                    await _context.MedicalTests.InsertOneAsync(newModel);
                    return _mapper.Map<MedicalTest, MedicalTestViewModel>(newModel);
                }

                // if the code get here then throwxeption
                throw new Exception("Something went wrong.");
            }
            catch (Exception)
            {
                //
                if (syncResult.Succeed)
                {
                    syncModel.IsAvailable = true;
                    SyncInstance(syncModel);
                }
                if (isBookingExam && examResult.Succeed)
                {
                    CancelBookingExamModel model = new CancelBookingExamModel()
                    {
                        InstanceId = newCreateModel.Instance.Id,
                        PersonId = newCreateModel.Customer.Id,
                    };
                    CancelBookingExamination(model);
                }
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hospitalId"></param>
        /// <param name="status"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public List<MedicalTestViewModel> GetSchedules(int hospitalId, int? status = null, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var basefilter = Builders<MedicalTest>.Filter.Empty;
                var hospitalIdFilter = Builders<MedicalTest>.Filter.Eq(mt => mt.Unit.Id, hospitalId);
                basefilter = basefilter & hospitalIdFilter;

                // filter status
                if (status.HasValue)
                {
                    var statusFilter = Builders<MedicalTest>.Filter.Eq(mt => mt.Status, status.Value);
                    basefilter = basefilter & statusFilter;
                }
                // filter date from
                //if (from.HasValue)
                //{
                //    var fromFilter = Builders<MedicalTest>.Filter.Gte(mt => mt.Instance.DateTime, from.Value);
                //    basefilter = basefilter & fromFilter;
                //}
                //// filter date to
                //if (to.HasValue)
                //{
                //    var toFilter = Builders<MedicalTest>.Filter.Lte(mt => mt.Instance.DateTime, to.Value);
                //    basefilter = basefilter & toFilter;
                //}

                // return
                List<MedicalTest> rs = _context.MedicalTests.Find(basefilter).ToList();
                return _mapper.Map<List<MedicalTest>, List<MedicalTestViewModel>>(rs);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="note"></param>
        public async Task<MedicalTestViewModel> Update(MedicalTestUpdateModel model)
        {
            using (var session = _context.StartSession())
            {
                session.StartTransaction();

                try
                {
                    // filter by Id
                    var filter = Builders<MedicalTest>.Filter.Eq(mt => mt.Id, model.Id);
                    // update status
                    var update = Builders<MedicalTest>.Update.Set(mt => mt.Status, model.Status);

                    // update note if any
                    if (!string.IsNullOrEmpty(model.Note))
                    {
                        update = update.Set(mt => mt.Note, model.Note);
                    }

                    // execute statement
                    await _context.MedicalTests.UpdateOneAsync(session, filter, update);
                    var modelUpdated = _context.MedicalTests.FindAsync(filter).Result.FirstOrDefault();

                    // try sync instance
                    if (model.Status.Equals(MedicalTestStatus.CANCELED) || model.Status.Equals(MedicalTestStatus.DOCTOR_CANCEL))
                    {
                        // create an instanceSync model 
                        InstanceSyncModel syncModel = new InstanceSyncModel()
                        {
                            Id = modelUpdated.Instance.Id,
                            IsAvailable = false,
                        };

                        // sync instance with MSSQL and api
                        var syncResponse = SyncInstance(syncModel);
                        var syncResult = JsonConvert.DeserializeObject<ResultModel>(syncResponse);

                        // create an exam cancel booking model
                        CancelBookingExamModel cancelModel = new CancelBookingExamModel()
                        {
                            PersonId = modelUpdated.Customer.Id,
                            InstanceId = modelUpdated.Instance.Id
                        };
                        // Cancel booking
                        var examResponse = CancelBookingExamination(cancelModel);
                        var examResult = JsonConvert.DeserializeObject<ResultModel>(examResponse);
                        // after sync success
                        if (syncResult.Succeed)
                        {
                            await session.CommitTransactionAsync();
                            return _mapper.Map<MedicalTest, MedicalTestViewModel>(modelUpdated);
                        }

                        // throw if fail to sync
                        throw new Exception(syncResult.ErrorMessage);
                    }

                    // commit transaction if status not canceled
                    await session.CommitTransactionAsync();
                    return _mapper.Map<MedicalTest, MedicalTestViewModel>(modelUpdated);
                }
                catch (Exception)
                {
                    await session.AbortTransactionAsync();
                    throw;
                }
            }
        }

        private string SyncInstance(InstanceSyncModel syncModel)
        {
            // to json
            var message = JsonConvert.SerializeObject(syncModel);

            //sync instance with MSSQL and api
            var response = _producer.Call(message, RabbitQueue.InstanceSyncQueue); // call and wait for response

            return response;
        }

        private string BookingExamination(BookingExamModel model)
        {
            // to json
            var message = JsonConvert.SerializeObject(model);

            var response = _producer.Call(message, RabbitQueue.ExaminationBookingQueue); // call and wait for response

            return response;
        }

        private string CancelBookingExamination(CancelBookingExamModel model)
        {
            // to json
            var message = JsonConvert.SerializeObject(model);

            var response = _producer.Call(message, RabbitQueue.CancelExaminationBookingQueue); // call and wait for response

            return response;
        }

        private string getUnitUsernameForExamination(string inputUsername)
        {
            var newUsername = inputUsername.Replace("hcdc.", "");
            if (inputUsername.Contains("hcdc.hcdc."))
                newUsername = inputUsername.Substring(5);

            return newUsername;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MedicalTestViewModel GetById(Guid id)
        {
            try
            {
                var result = _context.MedicalTests.Find(_ => _.Id == id).SingleOrDefault();

                return _mapper.Map<MedicalTest, MedicalTestViewModel>(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<MedicalTestViewModel> GetByCustomer(string username)
        {
            try
            {
                var result = _context.MedicalTests.Find(_ => _.BookedByUser == username).ToList();

                return _mapper.Map<List<MedicalTest>, List<MedicalTestViewModel>>(result);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<InstanceViewModel> GetInstances(int? id)
        {
            try
            {
                if (id.HasValue)
                {
                    var result = _context.Instances.Find(_ => _.Id == id).FirstOrDefault();
                    var list = new List<InstanceViewModel>();
                    var vm = _mapper.Map<Instance, InstanceViewModel>(result);
                    list.Add(vm);
                    return list;
                }
                else
                {
                    var instances = _context.Instances.AsQueryable().ToList();
                    return _mapper.Map<List<Instance>, List<InstanceViewModel>>(instances);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<ResultModel> GetFormFile(int medId)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        var file = await _context.FormFile.FindAsync(_ => _.MedId == medId);
        //        if (file.FirstOrDefault() == null)
        //        {
        //            throw new Exception("File not found");
        //        }
        //        result.Data = file.FirstOrDefault().Data;
        //        result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.Succeed = false;
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        //    }
        //    return result;
        //}

        //public async Task<ResultModel> CreateFormFile(FormFileCreateModel model)
        //{
        //    var result = new ResultModel();
        //    try
        //    {
        //        //var file = await _context.FormFile.FindAsync(_ => _.MedId == medId);
        //        //if (file.FirstOrDefault() == null)
        //        //{
        //        //    throw new Exception("File not found");
        //        //}
        //        //result.Data = file.FirstOrDefault().Data;
        //        //result.Succeed = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result.Succeed = false;
        //        result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
        //    }
        //    return result;
        //}
    }
}
