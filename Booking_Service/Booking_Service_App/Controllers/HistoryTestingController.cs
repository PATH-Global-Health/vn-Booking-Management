using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Services.Core;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
  //  [Authorize(AuthenticationSchemes = "Bearer")]
    public class HistoryTestingController : ControllerBase
    {
        private ITestingHistoryService _testingHistoryService;
        public HistoryTestingController(ITestingHistoryService testingHistoryService)
        {
            _testingHistoryService = testingHistoryService;
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
    }
}
