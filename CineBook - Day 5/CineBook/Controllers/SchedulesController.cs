using Microsoft.AspNetCore.Mvc;
using CineBook.Services;
using CineBook.Models;
using CineBook.DTOs;

namespace CineBook.Controllers
{
    [ApiController]
    [Route("api/schedules")]
    public class SchedulesController : ControllerBase
    {
        private readonly IScheduleService _scheduleService;

        public SchedulesController(IScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSchedules()
        {
            List<ScheduleResponse> schedules = await _scheduleService.GetAllAsync();
            return Ok(schedules);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetSchedule(long id)
        {
            ScheduleResponse schedule = await _scheduleService.GetByIdAsync(id);
            if (schedule == null) return NotFound();
            return Ok(schedule);
        }
    }
}
