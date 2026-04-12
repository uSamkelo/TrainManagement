using Microsoft.AspNetCore.Mvc;
using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Interfaces;
using TrainTrackingService.Models;

namespace TrainTrackingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainPositionController : ControllerBase
    {
        private readonly ITrainPositionEventPublisher _eventPublisher;
        private readonly ILogger<TrainPositionController> _logger;
        private readonly ITrainPositionService _trainPositionService;

        public TrainPositionController(
            ITrainPositionEventPublisher eventPublisher,
            ILogger<TrainPositionController> logger,
            ITrainPositionService trainPositionService)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _trainPositionService = trainPositionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainPositions(
            [FromQuery] string? station,
            [FromQuery] double? latitude,
            [FromQuery] double? longitude)
        {
            if (!string.IsNullOrEmpty(station))
                return Ok(await _trainPositionService.GetByStationAsync(station));

            if (latitude.HasValue && longitude.HasValue)
                return Ok(await _trainPositionService.GetByLocationAsync(latitude.Value, longitude.Value));

            return Ok(await _trainPositionService.GetAllAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainPosition position)
        {
            await _eventPublisher.PublishTrainPosition(position);
            await _trainPositionService.CreateAsync(position);
            return Ok(new { message = "Train position updated successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            var updated = await _trainPositionService.UpdateStatusAsync(id, status);
            if (!updated) return NotFound(new { error = "Train position not found" });
            return Ok(new { message = $"Status updated to {status}" });
        }

        [HttpPut("{id}/eta")]
        public async Task<IActionResult> UpdateETA(string id, [FromBody] TimeSpan eta)
        {
            var updated = await _trainPositionService.UpdateETAAsync(id, eta);
            if (!updated) return NotFound(new { error = "Train position not found" });
            return Ok(new { message = $"ETA updated to {eta}" });
        }
    }
}
