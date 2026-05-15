using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarfillAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsidyFieldsToOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OrderTime",
                table: "Orders",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(0) without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "GovernmentSubsidy",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrandTotal",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubsidyUsed",
                table: "Orders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NormalPricePerLitre",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubsidizedLiters",
                table: "Orders",
                type: "numeric(10,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubsidyPricePerLitre",
                table: "Orders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalBeforeSubsidy",
                table: "Orders",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GovernmentSubsidy",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "GrandTotal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsSubsidyUsed",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "NormalPricePerLitre",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubsidizedLiters",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubsidyPricePerLitre",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "SubtotalBeforeSubsidy",
                table: "Orders");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "OrderTime",
                table: "Orders",
                type: "time(0) without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<DateTime>(
                name: "OrderDate",
                table: "Orders",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
