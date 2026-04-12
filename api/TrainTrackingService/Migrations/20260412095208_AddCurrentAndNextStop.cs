using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainTrackingService.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentAndNextStop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TrainPositions",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CurrentStop",
                table: "TrainPositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextStop",
                table: "TrainPositions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentStop",
                table: "TrainPositions");

            migrationBuilder.DropColumn(
                name: "NextStop",
                table: "TrainPositions");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "TrainPositions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
