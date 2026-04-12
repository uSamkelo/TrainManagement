using Microsoft.AspNetCore.Mvc;
using TrainTrackingService.Data.DTOs;
using TrainTrackingService.Services.Interfaces;

namespace TrainTrackingService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrainRouteController : ControllerBase
    {
        private readonly ITrainRouteService _routeService;

        public TrainRouteController(ITrainRouteService routeService)
        {
            _routeService = routeService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RouteDTO>>> GetRoutes()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            var dtos = routes.Select(r => new RouteDTO
            {
                TrainId = r.Id,
                LineName = r.RouteName,
                TotalDistance = r.Stops.LastOrDefault()?.DistanceFromStart ?? 0,
                Stops = r.Stops.Select(s => new RouteStopDTO
                {
                    StationName = s.StationName,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    Distance = s.DistanceFromStart
                }).ToList()
            }).ToList();

            return Ok(dtos);
        }

        [HttpGet("{routeId}")]
        public async Task<IActionResult> GetRoute(string routeId)
        {
            var route = await _routeService.GetRouteAsync(routeId);
            if (route == null) return NotFound();

            var dto = new RouteDTO
            {
                TrainId = route.Id,
                LineName = route.RouteName,
                TotalDistance = route.Stops.LastOrDefault()?.DistanceFromStart ?? 0,
                Stops = route.Stops.Select(s => new RouteStopDTO
                {
                    StationName = s.StationName,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude,
                    Distance = s.DistanceFromStart
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpGet("stations")]
        public async Task<ActionResult<List<StationDTO>>> GetStations()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            var stations = routes
                .SelectMany(r => r.Stops)
                .GroupBy(s => s.StationName)
                .Select(g => g.First())
                .OrderBy(s => s.StationName)
                .Select(s => new StationDTO
                {
                    Id = s.Id,
                    StationName = s.StationName,
                    Latitude = s.Latitude,
                    Longitude = s.Longitude
                })
                .ToList();

            return Ok(stations);
        }
    }
}
