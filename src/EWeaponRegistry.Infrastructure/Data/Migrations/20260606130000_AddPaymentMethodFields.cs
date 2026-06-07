using System;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWeaponRegistry.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260606130000_AddPaymentMethodFields")]
    public partial class AddPaymentMethodFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "permit_applications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRejectionComment",
                table: "permit_applications",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "promise_applications",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentRejectionComment",
                table: "promise_applications",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "PaymentRejectionComment",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "promise_applications");

            migrationBuilder.DropColumn(
                name: "PaymentRejectionComment",
                table: "promise_applications");
        }
    }
}
