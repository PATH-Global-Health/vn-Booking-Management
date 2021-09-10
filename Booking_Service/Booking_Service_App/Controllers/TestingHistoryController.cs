using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Booking_Service_App.Extensions;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Services.Core;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TestingHistoryController : ControllerBase
    {
        private ITestingHistoryService _testingHistoryService;
        public TestingHistoryController(ITestingHistoryService testingHistoryService)
        {
            _testingHistoryService = testingHistoryService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _testingHistoryService.GetById(id);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] TestingHistoryCreateModel model)
        {
            try
            {
                var result = await _testingHistoryService.Add(model);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ResponseFailed);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        #region LayTest

        [HttpGet("LayTestById/{id}")]
        public async Task<IActionResult> GetLayTestById(Guid id)
        {
            try
            {
                var result = await _testingHistoryService.GetLayTestById(id);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("LayTest")]
        public async Task<IActionResult> Get(
            string employeeId, string employeeName, string customer, Guid? customerId = null, int? pageIndex=0,int? pageSize =0)
        {
            try
            {
                var result = await _testingHistoryService
                    .GetLayTest(employeeId, employeeName, customer, customerId.HasValue?customerId.Value: customerId,pageIndex,pageSize);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("LayTestByEmployee")]
        public async Task<IActionResult> GetLayTestByEmployee(string customerName, Guid? customerId = null)
        {
            try
            {
                var emp = User.Claims.Where(cl => cl.Type == "Id").FirstOrDefault().Value;
                var result = await _testingHistoryService.GetLayTestCustomer(emp, customerName, customerId.HasValue?customerId.Value:customerId);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("LayTestByCustomer")]
        public async Task<IActionResult> GetLayTestByCustomer(int? pageIndex = 0 , int? pageSize = 0)
        {
            try
            {
                var emp = User.Claims.Where(cl => cl.Type == "Id").FirstOrDefault().Value;
                var result = await _testingHistoryService.GetLayTestByCustomerId(emp,pageIndex,pageSize);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("LayTest")]
        public async Task<IActionResult> CreateLayTest([FromBody] LayTestCreateModel model)
        {
            try
            {
                var result = await _testingHistoryService.CreateLayTest(model);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        

        [HttpPut("LayTest")]
        public async Task<IActionResult> UpdateLayTest(LayTestUpdateModel model)
        {
            try
            {
                var result = await _testingHistoryService.UpdateLayTest(model);
                if (result.Succeed)
                {
                    return Ok(result.Data);
                }
                return BadRequest(result.ErrorMessage);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
        #endregion
    }
}
