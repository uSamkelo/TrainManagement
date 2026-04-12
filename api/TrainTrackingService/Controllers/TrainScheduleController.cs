using Microsoft.AspNetCore.Mvc;
using TrainTrackingService.Services.Interfaces;

namespace TrainTrackingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainScheduleController : ControllerBase
    {
        private readonly ITrainScheduleService _scheduleService;

        public TrainScheduleController(ITrainScheduleService scheduleService)
        {
            _scheduleService = scheduleService;
        }

        [HttpGet("next-arrival-eta")]
        public async Task<IActionResult> GetNextArrivalEta(
            [FromQuery] int? stopId,
            [FromQuery] double? latitude,
            [FromQuery] double? longitude)
        {
            var result = await _scheduleService.GetNextArrivalEtaAsync(stopId, latitude, longitude);
            if (result == null)
                return NotFound("No upcoming trains found for the given stop or location.");
            return Ok(result);
        }

        [HttpGet("day/{dayOfWeek}")]
        public async Task<IActionResult> GetDaySchedules(DayOfWeek dayOfWeek)
        {
            var schedules = await _scheduleService.GetDaySchedulesAsync(dayOfWeek);
            return Ok(schedules);
        }
    }
}
