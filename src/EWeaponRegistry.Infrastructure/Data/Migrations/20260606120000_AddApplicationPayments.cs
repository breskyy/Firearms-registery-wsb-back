using System;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWeaponRegistry.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260606120000_AddApplicationPayments")]
    public partial class AddApplicationPayments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "permit_applications",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 242m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "permit_applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceId",
                table: "permit_applications",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FeeAmount",
                table: "promise_applications",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 17m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "promise_applications",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PaymentReferenceId",
                table: "promise_applications",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "promise_application_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PromiseApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttachmentType = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Content = table.Column<byte[]>(type: "bytea", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promise_application_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promise_application_attachments_promise_applications_PromiseApplicationId",
                        column: x => x.PromiseApplicationId,
                        principalTable: "promise_applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_promise_application_attachments_PromiseApplicationId_AttachmentType",
                table: "promise_application_attachments",
                columns: new[] { "PromiseApplicationId", "AttachmentType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "promise_application_attachments");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "PaymentReferenceId",
                table: "permit_applications");

            migrationBuilder.DropColumn(
                name: "FeeAmount",
                table: "promise_applications");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "promise_applications");

            migrationBuilder.DropColumn(
                name: "PaymentReferenceId",
                table: "promise_applications");
        }
    }
}
