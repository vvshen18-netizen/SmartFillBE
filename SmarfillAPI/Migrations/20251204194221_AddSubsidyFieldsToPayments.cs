using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmarfillAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsidyFieldsToPayments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PaymentTime",
                table: "Payments",
                type: "interval",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time(0) without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<decimal>(
                name: "GovernmentSubsidy",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "GrandTotal",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubsidyUsed",
                table: "Payments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "NormalPricePerLitre",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubsidizedLiters",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubsidyPricePerLitre",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SubtotalBeforeSubsidy",
                table: "Payments",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GovernmentSubsidy",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "GrandTotal",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsSubsidyUsed",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "NormalPricePerLitre",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SubsidizedLiters",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SubsidyPricePerLitre",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SubtotalBeforeSubsidy",
                table: "Payments");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "PaymentTime",
                table: "Payments",
                type: "time(0) without time zone",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "interval");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PaymentDate",
                table: "Payments",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
