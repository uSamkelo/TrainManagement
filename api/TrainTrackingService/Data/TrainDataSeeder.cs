using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainTrackingService.Models;

namespace TrainTrackingService.Data
{
    public static class TrainDataSeeder
    {
        public static async Task SeedAsync(TrainTrackingContext context)
        {
            // Seed routes if none exist
            if (!await context.TrainRoutes.AnyAsync())
            {
                var routes = GetRoutes();
                context.TrainRoutes.AddRange(routes);
                await context.SaveChangesAsync();
            }

            // Seed stops if none exist (may have been missed in a previous run)
            if (!await context.TrainStops.AnyAsync())
            {
                var stops = GetStops();
                context.TrainStops.AddRange(stops);
                await context.SaveChangesAsync();
            }

            // Seed schedules if none exist
            if (!await context.TrainSchedules.AnyAsync())
            {
                var routes = await context.TrainRoutes.ToListAsync();
                var stops = await context.TrainStops.ToListAsync();
                var (schedules, _) = BuildWeeklySchedules(routes, stops);
                context.TrainSchedules.AddRange(schedules);
                await context.SaveChangesAsync();
            }
        }

        // ── Routes ─────────────────────────────────────────────────────────
        private static List<TrainRoute> GetRoutes() => new()
        {
            new TrainRoute { Id = "SouthernLine",   RouteName = "Southern Line (Cape Town – Simon's Town)",        LineColor = "#e67e22" },
            new TrainRoute { Id = "NorthernLine",   RouteName = "Northern Line (Cape Town – Bellville)",           LineColor = "#1abc9c" },
            new TrainRoute { Id = "CapeFlatsLine",  RouteName = "Cape Flats Line (Cape Town – Mitchells Plain)",   LineColor = "#9b59b6" },
        };

        // ── Stops (real Cape Town Metrorail coordinates) ───────────────────
        private static List<TrainStop> GetStops()
        {
            int id = 1;

            // ─── Southern Line ──────────────────────────────────────────────
            var southern = new (string name, double lat, double lon, double km)[]
            {
                ("Cape Town",    -33.9234, 18.4262,  0.0),
                ("Woodstock",    -33.9284, 18.4414,  1.8),
                ("Salt River",   -33.9356, 18.4619,  3.7),
                ("Observatory",  -33.9458, 18.4719,  5.1),
                ("Mowbray",      -33.9530, 18.4730,  6.2),
                ("Rosebank",     -33.9580, 18.4720,  7.0),
                ("Rondebosch",   -33.9620, 18.4718,  7.5),
                ("Newlands",     -33.9634, 18.4717,  8.8),
                ("Claremont",    -33.9748, 18.4712, 10.4),
                ("Harfield Road",-33.9810, 18.4700, 11.2),
                ("Kenilworth",   -33.9890, 18.4680, 12.4),
                ("Wynberg",      -34.0041, 18.4646, 14.2),
                ("Plumstead",    -34.0150, 18.4610, 15.8),
                ("Steurhof",     -34.0250, 18.4590, 16.8),
                ("Retreat",      -34.0536, 18.4529, 20.2),
                ("Steenberg",    -34.0690, 18.4590, 21.8),
                ("Lakeside",     -34.0810, 18.4620, 23.0),
                ("Muizenberg",   -34.1086, 18.4690, 26.5),
                ("St James",     -34.1170, 18.4580, 27.8),
                ("Kalk Bay",     -34.1281, 18.4503, 29.2),
                ("Fish Hoek",    -34.1414, 18.4326, 31.3),
                ("Glencairn",    -34.1580, 18.4310, 33.2),
                ("Simon's Town", -34.1868, 18.4277, 37.1),
            };

            // ─── Northern Line ──────────────────────────────────────────────
            var northern = new (string name, double lat, double lon, double km)[]
            {
                ("Cape Town",    -33.9234, 18.4262,  0.0),
                ("Woodstock",    -33.9284, 18.4414,  1.8),
                ("Salt River",   -33.9356, 18.4619,  3.7),
                ("Maitland",     -33.9254, 18.4878,  6.2),
                ("Ndabeni",      -33.9180, 18.5010,  7.5),
                ("Pinelands",    -33.9250, 18.5170,  9.1),
                ("Goodwood",     -33.9048, 18.5304, 12.1),
                ("Vasco",        -33.9015, 18.5445, 13.6),
                ("Parow",        -33.9045, 18.5601, 15.3),
                ("Elsies River", -33.9010, 18.5720, 16.6),
                ("Tygerberg",    -33.8980, 18.5880, 18.2),
                ("Bellville",    -33.9019, 18.6300, 22.1),
                ("Stikland",     -33.8900, 18.6500, 24.3),
                ("Brackenfell",  -33.8750, 18.6780, 27.5),
                ("Kraaifontein", -33.8654, 18.7062, 31.4),
            };

            // ─── Cape Flats Line ────────────────────────────────────────────
            var capeFlats = new (string name, double lat, double lon, double km)[]
            {
                ("Cape Town",        -33.9234, 18.4262,  0.0),
                ("Woodstock",        -33.9284, 18.4414,  1.8),
                ("Salt River",       -33.9356, 18.4619,  3.7),
                ("Koeberg Road",     -33.9310, 18.4780,  5.2),
                ("Maitland",         -33.9254, 18.4878,  6.2),
                ("Langa",            -33.9420, 18.5310,  9.8),
                ("Bonteheuwel",      -33.9510, 18.5520, 11.5),
                ("Athlone",          -33.9630, 18.5050, 13.2),
                ("Heideveld",        -33.9710, 18.5590, 15.0),
                ("Nyanga",           -33.9880, 18.5730, 17.2),
                ("Philippi",         -34.0100, 18.5800, 19.8),
                ("Mitchells Plain",  -34.0450, 18.6160, 25.3),
                ("Khayelitsha",      -34.0390, 18.6770, 30.8),
            };

            var allStops = new List<TrainStop>();

            foreach (var (data, routeId) in new[] { (southern, "SouthernLine"), (northern, "NorthernLine"), (capeFlats, "CapeFlatsLine") })
            {
                int order = 1;
                foreach (var s in data)
                {
                    allStops.Add(new TrainStop
                    {
                        Id = id++,
                        StationName = s.name,
                        Latitude = s.lat,
                        Longitude = s.lon,
                        StopOrder = order++,
                        DistanceFromStart = s.km,
                        RouteId = routeId
                    });
                }
            }

            return allStops;
        }

        // ── Weekly Schedule Builder ────────────────────────────────────────
        // Weekdays: trains every 20 min peak (05:00-08:40, 15:40-19:00), every 40 min off-peak
        // Weekends: trains every 60 min, 06:00-20:00
        private static (List<TrainSchedule> schedules, List<ScheduleStop> stops) BuildWeeklySchedules(
            List<TrainRoute> routes, List<TrainStop> stops)
        {
            var schedules = new List<TrainSchedule>();
            var allScheduleStops = new List<ScheduleStop>();

            foreach (var route in routes)
            {
                var routeStops = stops.Where(s => s.RouteId == route.Id).OrderBy(s => s.StopOrder).ToList();

                for (int day = 0; day < 7; day++)
                {
                    var dayOfWeek = (DayOfWeek)day;
                    var departures = GetDepartureTimes(dayOfWeek);
                    int trainNum = 1;

                    foreach (var departure in departures)
                    {
                        string trainId = $"{route.Id}_T{trainNum:D2}";

                        var schedule = new TrainSchedule
                        {
                            TrainId = trainId,
                            RouteId = route.Id,
                            DayOfWeek = dayOfWeek,
                            Stops = new List<ScheduleStop>()
                        };

                        // Average speed ~40 km/h for metro => ~1.5 min/km
                        foreach (var stop in routeStops)
                        {
                            var travelMinutes = stop.DistanceFromStart * 1.5;
                            var arrival = departure.Add(TimeSpan.FromMinutes(travelMinutes));
                            var departureTime = arrival.Add(TimeSpan.FromMinutes(1)); // 1 min dwell

                            schedule.Stops.Add(new ScheduleStop
                            {
                                StopId = stop.Id,
                                ArrivalTime = arrival,
                                DepartureTime = departureTime,
                            });
                        }

                        schedules.Add(schedule);
                        trainNum++;
                    }
                }
            }

            return (schedules, allScheduleStops);
        }

        private static List<TimeSpan> GetDepartureTimes(DayOfWeek day)
        {
            var departures = new List<TimeSpan>();
            bool isWeekend = day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;

            if (isWeekend)
            {
                // Weekend: hourly 06:00 – 20:00
                for (int h = 6; h <= 20; h++)
                    departures.Add(TimeSpan.FromHours(h));
            }
            else
            {
                // Morning peak: 05:00 – 08:40 every 20 min
                for (var t = TimeSpan.FromHours(5); t <= TimeSpan.FromHours(8).Add(TimeSpan.FromMinutes(40)); t = t.Add(TimeSpan.FromMinutes(20)))
                    departures.Add(t);

                // Off-peak: 09:00 – 15:20 every 40 min
                for (var t = TimeSpan.FromHours(9); t <= TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(20)); t = t.Add(TimeSpan.FromMinutes(40)))
                    departures.Add(t);

                // Evening peak: 15:40 – 19:00 every 20 min
                for (var t = TimeSpan.FromHours(15).Add(TimeSpan.FromMinutes(40)); t <= TimeSpan.FromHours(19); t = t.Add(TimeSpan.FromMinutes(20)))
                    departures.Add(t);

                // Late off-peak: 19:30 – 21:30 every 60 min
                for (var t = TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(30)); t <= TimeSpan.FromHours(21).Add(TimeSpan.FromMinutes(30)); t = t.Add(TimeSpan.FromMinutes(60)))
                    departures.Add(t);
            }

            return departures;
        }
    }
}
