﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.ViewModels;
using Services.Core;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = "Bearer")]
    public class ARTController : ControllerBase
    {
        private IARTService _artService;

        public ARTController(IARTService artService)
        {
            _artService = artService;
        }

        [HttpGet("GetByCustomerId/{customerId}")]
        public async Task<IActionResult> Get(Guid customerId)
        {
            try
            {
                var result = await _artService.GetByCustomerId(customerId);
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
        public async Task<IActionResult> Add([FromBody] ARTCreateModel model)
        {
            try
            {
                var result = await _artService.Add(model);
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
