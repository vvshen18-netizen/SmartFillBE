using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarfillAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveRejectionReasonFromOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Orders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Orders",
                type: "text",
                nullable: true);
        }
    }
}
