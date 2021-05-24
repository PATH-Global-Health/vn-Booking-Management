using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Booking_Service_App.Extensions;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;
using Services.Core;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class VaccinationsController : Controller
    {
        private IVaccinationService _vaccinationService;

        public VaccinationsController(IVaccinationService service)
        {
            _vaccinationService = service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] VaccinationCreateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = await _vaccinationService.Add(model, username);
                if (result.Succeed)
                    return Ok(result);
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
        public IActionResult Get(Guid? unitId, int? status = null, DateTime? from = null, DateTime? to = null) //For Unit and Doctor
        {
            try
            {
                var username = User.Claims.GetUsername();
                ResultModel result = new ResultModel();
                if (unitId.HasValue)
                {
                    result = _vaccinationService.Get(unitId.Value, status, from, to);
                }
                else
                {
                    result = _vaccinationService.GetByCustomer(username);
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
        public IActionResult GetById(Guid id)
        {
            try
            {
                var result = _vaccinationService.GetById(id);
                if (result.Succeed)
                    return Ok(result);
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
        public IActionResult Update([FromBody] VaccinationUpdateModel model)
        {
            try
            {
                var username = User.Claims.GetUsername();
                var result = _vaccinationService.Update(model);
                if (result.Succeed)
                    return Ok(result);
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Statistic")]
        [AllowAnonymous]
        public async Task<IActionResult> Statistic(Guid unitId, DateTime? from = null, DateTime? to = null)
        {
            var result = await _vaccinationService.Statistic(unitId, from, to);
            if (result.Succeed)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.ErrorMessage);
        }
    }
}
