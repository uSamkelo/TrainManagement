using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class AddETAField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "ETA",
                table: "TrainPositions",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ETA",
                table: "TrainPositions");
        }
    }
}
