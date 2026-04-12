using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToTrainPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "TrainPositions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TrainPositions");
        }
    }
}
