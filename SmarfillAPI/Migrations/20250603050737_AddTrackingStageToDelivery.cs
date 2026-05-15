using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarfillAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTrackingStageToDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrackingStage",
                table: "Deliveries",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackingStage",
                table: "Deliveries");
        }
    }
}
