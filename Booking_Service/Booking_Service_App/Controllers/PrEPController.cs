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
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class PrEPController : ControllerBase
    {
        private IPrEPService _prEPService;

        public PrEPController(IPrEPService prEPService)
        {
            _prEPService = prEPService;
        }

        [HttpGet("GetByCustomerId/{customerId}")]
        public async Task<IActionResult> Get(Guid customerId,string unitId)
        {
            try
            {
                var result = await _prEPService.GetByCustomerId(customerId,unitId);
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
        public async Task<IActionResult> Add([FromBody] PrEPCreateModel model)
        {
            try
            {
                var result = await _prEPService.Add(model);
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

        [HttpPut]
        public async Task<IActionResult> UpdatePrEP(Guid id,[FromBody] PrEPUpdateModel model)
        {
            try
            {
                var result = await _prEPService.UpdatePrEP(id, model);
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
    }
}
