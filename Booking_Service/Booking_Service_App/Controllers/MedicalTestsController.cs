using Data.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Services;
using System;
using System.Collections.Generic;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicalTestsController : ControllerBase
    {
        private IMedicalTestService _medicalTestService;
        private IJwtHandler _jwtHandler;

        public MedicalTestsController(IMedicalTestService medicalTestService, IJwtHandler jwtHandler)
        {
            _medicalTestService = medicalTestService;
            _jwtHandler = jwtHandler;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Register([FromBody] MedicalTestCreateModel model)
        {
            try
            {
                Request.Headers.TryGetValue("Authorization", out var jwt);
                var username = _jwtHandler.GetUsername(jwt.ToString());

                var result = _medicalTestService.AddTimeBooked(model, username).Result;
                return Ok(result);
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
        public IActionResult GetSchedules(int? status = null, DateTime? from = null, DateTime? to = null) //For Unit and Doctor
        {
            try
            {
                //var token = _jwtHandler.GenerateJwtToken(4, "dương");
                // handle jwt token and get hospitalId
                Request.Headers.TryGetValue("Authorization", out var jwt);
                var hospitalId = _jwtHandler.GetHospitalId(jwt.ToString());
                var username = _jwtHandler.GetUsername(jwt.ToString());

                List<MedicalTestViewModel> list = new List<MedicalTestViewModel>();

                if (hospitalId > 0)
                {
                    list = _medicalTestService.GetSchedules(hospitalId, status, from, to);
                }
                else if (!string.IsNullOrEmpty(username))
                {
                    list = _medicalTestService.GetByCustomer(username);
                }
                return Ok(list);
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
                var result = _medicalTestService.GetById(id);
                return Ok(result);
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
        public IActionResult UpdateSchedules([FromBody] MedicalTestUpdateModel model)
        {
            try
            {
                Request.Headers.TryGetValue("Authorization", out var jwt);
                var hospitalId = _jwtHandler.GetHospitalId(jwt.ToString());
                var username = _jwtHandler.GetUsername(jwt.ToString());
                var rs = _medicalTestService.UpdateTimeBooked(model).Result;
                return Ok(rs);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("GetInstances")]
        public IActionResult GetInstances(int? id)
        {
            try
            {
                var result = _medicalTestService.GetInstances(id);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}