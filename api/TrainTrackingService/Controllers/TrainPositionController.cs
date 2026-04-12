using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using TrainTrackingService.Data;
using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Models;

namespace TrainTrackingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainPositionController : ControllerBase
    {
        private readonly ITrainPositionEventPublisher _eventPublisher;
        private readonly ILogger<TrainPositionController> _logger;
        private readonly TrainTrackingContext _context;

        public TrainPositionController(ITrainPositionEventPublisher eventPublisher, 
            ILogger<TrainPositionController> logger, TrainTrackingContext context)
        {
            _eventPublisher = eventPublisher;
            _logger = logger;
            _context = context;
        }

        // GET: /trainposition
        // [HttpGet]
        // public IActionResult Get()
        // {
        //     var positions = _context.TrainPositions.ToList();
        //     return Ok(positions);
        // }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TrainPositionDTO>>> GetTrainPositions()
        {
            var positions = await _context.TrainPositions.ToListAsync();
            return positions.Select(p => new TrainPositionDTO
            {
                TrainId = p.TrainId,
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Status = p.Status,
                ETA = p.ETA.ToString(@"hh\:mm") // Convert TimeSpan to "HH:mm"
            }).ToList();
        }

        // POST: /trainposition
        [HttpPost]
        public IActionResult Post([FromBody] TrainPosition position)
        {
            try
            {
                // Publish event to Azure Service Bus
                _eventPublisher.PublishTrainPosition(position);

                // Save to SQL database
                _context.TrainPositions.Add(position);
                _context.SaveChanges();
                return Ok(new { message = "Train position updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing train position event");
                return StatusCode(500, new { error = "Failed to update train position" });
            }
        }

        // PUT: /trainposition/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateStatus(string id, [FromBody] string status)
        {
            try
            {
                var position = _context.TrainPositions.FirstOrDefault(p => p.TrainId == id);
                if (position == null)
                {
                    return NotFound(new { error = "Train position not found" });
                }

                position.Status = status;
                _context.SaveChanges();
                return Ok(new { message = $"Status updated to {status}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating train position status");
                return StatusCode(500, new { error = "Failed to update train position status" });
            }
        }

        // PUT: /trainposition/{id}/eta
        [HttpPut("{id}/eta")]
        public IActionResult UpdateETA(string id, [FromBody] TimeSpan eta)
        {
            try
            {
                var position = _context.TrainPositions.FirstOrDefault(p => p.TrainId == id);
                if (position == null)
                {
                    return NotFound(new { error = "Train position not found" });
                }

                position.ETA = eta;
                _context.SaveChanges();
                return Ok(new { message = $"ETA updated to {eta}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating train ETA");
                return StatusCode(500, new { error = "Failed to update train ETA" });
            }
        }
    }
}
