using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TrainTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class AddDataSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrainRoute",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RouteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LineColor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainRoute", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainSchedules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrainId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RouteId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainSchedules_TrainRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TrainRoute",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainStops",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StationName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    StopOrder = table.Column<int>(type: "int", nullable: false),
                    DistanceFromStart = table.Column<double>(type: "float", nullable: false),
                    RouteId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainStops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrainStops_TrainRoute_RouteId",
                        column: x => x.RouteId,
                        principalTable: "TrainRoute",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ScheduleStop",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StopId = table.Column<int>(type: "int", nullable: false),
                    TrainScheduleId = table.Column<int>(type: "int", nullable: false),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    DepartureTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleStop", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleStop_TrainSchedules_TrainScheduleId",
                        column: x => x.TrainScheduleId,
                        principalTable: "TrainSchedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleStop_TrainStops_StopId",
                        column: x => x.StopId,
                        principalTable: "TrainStops",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "TrainRoute",
                columns: new[] { "Id", "LineColor", "RouteName" },
                values: new object[,]
                {
                    { "CT1", "#1abc9c", "Cape Town - Bellville" },
                    { "CT2", "#e67e22", "Cape Town - Simon's Town" }
                });

            migrationBuilder.InsertData(
                table: "TrainSchedules",
                columns: new[] { "Id", "DayOfWeek", "RouteId", "TrainId" },
                values: new object[,]
                {
                    { 1, 0, "CT1", "CT1-T1" },
                    { 2, 1, "CT1", "CT1-T1" },
                    { 3, 2, "CT1", "CT1-T1" },
                    { 4, 3, "CT1", "CT1-T1" },
                    { 5, 4, "CT1", "CT1-T1" },
                    { 6, 5, "CT1", "CT1-T1" },
                    { 7, 6, "CT1", "CT1-T1" },
                    { 8, 0, "CT1", "CT1-T2" },
                    { 9, 1, "CT1", "CT1-T2" },
                    { 10, 2, "CT1", "CT1-T2" },
                    { 11, 3, "CT1", "CT1-T2" },
                    { 12, 4, "CT1", "CT1-T2" },
                    { 13, 5, "CT1", "CT1-T2" },
                    { 14, 6, "CT1", "CT1-T2" },
                    { 15, 0, "CT2", "CT2-T1" },
                    { 16, 1, "CT2", "CT2-T1" },
                    { 17, 2, "CT2", "CT2-T1" },
                    { 18, 3, "CT2", "CT2-T1" },
                    { 19, 4, "CT2", "CT2-T1" },
                    { 20, 5, "CT2", "CT2-T1" },
                    { 21, 6, "CT2", "CT2-T1" },
                    { 22, 0, "CT2", "CT2-T2" },
                    { 23, 1, "CT2", "CT2-T2" },
                    { 24, 2, "CT2", "CT2-T2" },
                    { 25, 3, "CT2", "CT2-T2" },
                    { 26, 4, "CT2", "CT2-T2" },
                    { 27, 5, "CT2", "CT2-T2" },
                    { 28, 6, "CT2", "CT2-T2" }
                });

            migrationBuilder.InsertData(
                table: "TrainStops",
                columns: new[] { "Id", "DistanceFromStart", "Latitude", "Longitude", "RouteId", "StationName", "StopOrder" },
                values: new object[,]
                {
                    { 1, 0.0, -33.925800000000002, 18.423200000000001, "CT1", "Cape Town", 1 },
                    { 2, 3.5, -33.928100000000001, 18.455100000000002, "CT1", "Salt River", 2 },
                    { 3, 22.5, -33.899099999999997, 18.629799999999999, "CT1", "Bellville", 3 },
                    { 4, 0.0, -33.925800000000002, 18.423200000000001, "CT2", "Cape Town", 1 },
                    { 5, 27.5, -34.1051, 18.4697, "CT2", "Muizenberg", 7 },
                    { 6, 43.5, -34.193100000000001, 18.432600000000001, "CT2", "Simon's Town", 12 }
                });

            migrationBuilder.InsertData(
                table: "ScheduleStop",
                columns: new[] { "Id", "ArrivalTime", "DepartureTime", "StopId", "TrainScheduleId" },
                values: new object[,]
                {
                    { 1, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 1 },
                    { 2, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 1 },
                    { 3, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 1 },
                    { 4, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 2 },
                    { 5, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 2 },
                    { 6, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 2 },
                    { 7, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 3 },
                    { 8, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 3 },
                    { 9, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 3 },
                    { 10, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 4 },
                    { 11, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 4 },
                    { 12, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 4 },
                    { 13, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 5 },
                    { 14, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 5 },
                    { 15, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 5 },
                    { 16, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 6 },
                    { 17, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 6 },
                    { 18, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 6 },
                    { 19, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 1, 7 },
                    { 20, new TimeSpan(0, 8, 15, 0, 0), new TimeSpan(0, 8, 17, 0, 0), 2, 7 },
                    { 21, new TimeSpan(0, 8, 30, 0, 0), new TimeSpan(0, 8, 32, 0, 0), 3, 7 },
                    { 22, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 8 },
                    { 23, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 8 },
                    { 24, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 8 },
                    { 25, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 9 },
                    { 26, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 9 },
                    { 27, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 9 },
                    { 28, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 10 },
                    { 29, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 10 },
                    { 30, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 10 },
                    { 31, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 11 },
                    { 32, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 11 },
                    { 33, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 11 },
                    { 34, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 12 },
                    { 35, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 12 },
                    { 36, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 12 },
                    { 37, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 13 },
                    { 38, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 13 },
                    { 39, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 13 },
                    { 40, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 1, 14 },
                    { 41, new TimeSpan(0, 10, 15, 0, 0), new TimeSpan(0, 10, 17, 0, 0), 2, 14 },
                    { 42, new TimeSpan(0, 10, 30, 0, 0), new TimeSpan(0, 10, 32, 0, 0), 3, 14 },
                    { 43, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 15 },
                    { 44, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 15 },
                    { 45, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 15 },
                    { 46, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 16 },
                    { 47, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 16 },
                    { 48, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 16 },
                    { 49, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 17 },
                    { 50, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 17 },
                    { 51, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 17 },
                    { 52, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 18 },
                    { 53, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 18 },
                    { 54, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 18 },
                    { 55, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 19 },
                    { 56, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 19 },
                    { 57, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 19 },
                    { 58, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 20 },
                    { 59, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 20 },
                    { 60, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 20 },
                    { 61, new TimeSpan(0, 8, 0, 0, 0), new TimeSpan(0, 8, 2, 0, 0), 4, 21 },
                    { 62, new TimeSpan(0, 9, 30, 0, 0), new TimeSpan(0, 9, 32, 0, 0), 5, 21 },
                    { 63, new TimeSpan(0, 10, 45, 0, 0), new TimeSpan(0, 10, 47, 0, 0), 6, 21 },
                    { 64, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 22 },
                    { 65, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 22 },
                    { 66, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 22 },
                    { 67, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 23 },
                    { 68, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 23 },
                    { 69, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 23 },
                    { 70, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 24 },
                    { 71, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 24 },
                    { 72, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 24 },
                    { 73, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 25 },
                    { 74, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 25 },
                    { 75, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 25 },
                    { 76, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 26 },
                    { 77, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 26 },
                    { 78, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 26 },
                    { 79, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 27 },
                    { 80, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 27 },
                    { 81, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 27 },
                    { 82, new TimeSpan(0, 10, 0, 0, 0), new TimeSpan(0, 10, 2, 0, 0), 4, 28 },
                    { 83, new TimeSpan(0, 11, 30, 0, 0), new TimeSpan(0, 11, 32, 0, 0), 5, 28 },
                    { 84, new TimeSpan(0, 12, 45, 0, 0), new TimeSpan(0, 12, 47, 0, 0), 6, 28 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleStop_StopId",
                table: "ScheduleStop",
                column: "StopId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleStop_TrainScheduleId",
                table: "ScheduleStop",
                column: "TrainScheduleId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainSchedules_RouteId",
                table: "TrainSchedules",
                column: "RouteId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainStops_RouteId",
                table: "TrainStops",
                column: "RouteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleStop");

            migrationBuilder.DropTable(
                name: "TrainSchedules");

            migrationBuilder.DropTable(
                name: "TrainStops");

            migrationBuilder.DropTable(
                name: "TrainRoute");
        }
    }
}
