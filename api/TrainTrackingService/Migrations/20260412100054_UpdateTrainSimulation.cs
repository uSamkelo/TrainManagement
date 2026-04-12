using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrainSimulation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "NextStopETA",
                table: "TrainPositions",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NextStopETA",
                table: "TrainPositions");
        }
    }
}
