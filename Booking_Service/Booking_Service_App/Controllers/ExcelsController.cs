using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Core;
using System;
using System.Threading.Tasks;

namespace Booking_Service_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ExcelsController : ControllerBase
    {
        private readonly IExcelService _excelService;

        public ExcelsController(IExcelService excelService)
        {
            _excelService = excelService;
        }

        [HttpGet("ExamReport")]
        public async Task<IActionResult> ExamReport(Guid unitId, DateTime dateTaken)
        {
            var result = await _excelService.ExamReport(unitId, dateTaken);
            if (result.Succeed)
            {
                var fileBytes = result.Data as byte[];
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report.xlsx");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("AvailableDatesForExamReport")]
        public async Task<IActionResult> AvailableDatesForExamReport(Guid unitId)
        {
            var result = await _excelService.AvailableDatesForExamReport(unitId);
            if (result.Succeed)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("VaccReport")]
        public async Task<IActionResult> VaccReport(Guid unitId, DateTime fromTime, DateTime toTime)
        {
            var result = await _excelService.VaccReport(unitId, fromTime, toTime);
            if (result.Succeed)
            {
                var fileBytes = result.Data as byte[];
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"report.xlsx");
            }
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("AvailableDatesForVaccReport")]
        public async Task<IActionResult> AvailableDatesForVaccReport(Guid unitId)
        {
            var result = await _excelService.AvailableDatesForVaccReport(unitId);
            if (result.Succeed)
            {
                return Ok(result.Data);
            }
            return BadRequest(result.ErrorMessage);
        }
    }
}
