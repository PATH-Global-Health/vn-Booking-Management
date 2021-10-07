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
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class WorkingSessionController : ControllerBase
    {
        private IWorkingSessionService _workingSessionService;

        public WorkingSessionController(IWorkingSessionService workingSessionService)
        {
            _workingSessionService = workingSessionService;
        }

        [HttpGet("GetSessionEmployee")]
        public async Task<IActionResult> Get(Guid? customerId=null)
        {
            try
            {
                var emp = User.Claims.Where(cl => cl.Type == "Id").FirstOrDefault().Value;
                var result = await _workingSessionService.FilterByEmployee(emp, customerId);
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

        [HttpGet("SessionByCustomerId")]
        public async Task<IActionResult> GetByCustomerId(Guid customerId)
        {
            try
            {
                var emp = User.Claims.Where(cl => cl.Type == "Id").FirstOrDefault().Value;
                var result = await _workingSessionService.GetSessionByCustomerId(emp, customerId);
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
        public async Task<IActionResult> Add([FromBody] WorkingSessionCreateModel model)
        {
            try
            {
                var result = await _workingSessionService.CreateSession(model);
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

//        [HttpPost("TestRabbit")]
//        [AllowAnonymous]
//        public  IActionResult AddTick([FromBody] TicketEmployeeModel model)
//        {
//            try
//            {
//                var result =  _workingSessionService.TestRabit(model);
//                if (result.Succeed)
//                {
//                    return Ok(result.Data);
//                }
//                return BadRequest(result.ErrorMessage);
//            }
//            catch (Exception e)
//            {
//                return BadRequest(e.Message);
//            }
//        }


    }
}
