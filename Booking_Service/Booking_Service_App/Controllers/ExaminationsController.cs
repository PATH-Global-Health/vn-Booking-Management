using Booking_Service_App.Extensions;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Core;
using System;
using System.Threading.Tasks;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ExaminationsController : ControllerBase
    {
        private IExaminationService _examService;

        public ExaminationsController(IExaminationService examService)
        {
            _examService = examService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] ExaminationCreateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.Add(model, username);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get(Guid? unitId = null, int? status = null, DateTime? from = null, DateTime? to = null,Guid? doctorId = null) //For Unit and Doctor
        {
            try
            {
                var username = User.Claims.GetUsername();
                ResultModel result = new ResultModel();
                if (unitId.HasValue)
                {
                    result = await _examService.Get(unitId.Value, status, from, to);
                }
                else if (doctorId.HasValue)
                {
                    result = await _examService.Get(unitId, status, from, to, doctorId.Value);
                }
                else
                {
                    result = await _examService.GetByCustomer(username);
                }
                if (result.Succeed)
                {
                    return Ok(result);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _examService.GetById(id);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] ExaminationUpdateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.Update(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody] ExaminationDeleteModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.Delete(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPut("UpdateResult")]
        public async Task<IActionResult> UpdateResult([FromBody] ExaminationUpdateResultModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.UpdateResult(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("CreateResultForm")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateResultForm([FromForm] FormFileCreateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.CreateResultForm(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut("UpdateResultForm")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateResultForm([FromForm] FormFileUpdateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.UpdateResultForm(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }

                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("ResultForm")]
        public async Task<IActionResult> ResultForm(Guid examId)
        {
            var result = await _examService.GetResultForm(examId);
            if (result.Succeed)
            {
                var fileBytes = result.Data as byte[];
                return File(fileBytes, "application/pdf", $"result.pdf");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpPut("Rating")]
        public async Task<IActionResult> ExaminationRating(ExaminationRatingModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _examService.Rating(model);
                if (result.Succeed)
                {
                    return Ok(result);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //[HttpGet("UpdateDB")]
        //[AllowAnonymous]
        //public async Task<IActionResult> UpdateDB()
        //{
        //    var result = await _examService.UpdateDB();
        //    if (result.Succeed)
        //    {
        //        return Ok();
        //    }
        //    return BadRequest(result.ErrorMessage);
        //}

        [HttpGet("Statistic")]
        [AllowAnonymous]
        public async Task<IActionResult> Statistic(Guid? unitId = null, Guid? doctorId = null, DateTime? from = null, DateTime? to = null)
        {
            ResultModel result = new ResultModel();

            if (unitId.HasValue)
            {
                 result = await _examService.Statistic(unitId.Value, from, to);
            }
            else if(doctorId.HasValue)
            {
                result = await _examService.Statistic(unitId, from, to, doctorId.Value);
            }
            else if(unitId == null && doctorId == null )
            {
                return BadRequest("UnitId and DoctorId is null");
            }
            
            if (result.Succeed)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.ErrorMessage);
        }

//        [HttpPost("TestRabit")]
//        [AllowAnonymous]
//        public IActionResult TestRabbit([FromBody] IntervalSyncModel model)
//        {
//            var result =  _examService.TestRabit(model);
//            if (result.Succeed)
//            {
//                return Ok(result.Data);
//            }
//            return BadRequest(result.ErrorMessage);
//        }


    }
}
