using System;
using EWeaponRegistry.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWeaponRegistry.Infrastructure.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260604120000_AddPermitMedicalExamRenewals")]
    public partial class AddPermitMedicalExamRenewals : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permit_medical_exam_renewals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitId = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ProposedMedicalExpiryDateEncrypted = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ProposedPsychologicalExpiryDateEncrypted = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReviewedByOfficerUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permit_medical_exam_renewals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permit_medical_exam_renewals_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_permit_medical_exam_renewals_permits_PermitId",
                        column: x => x.PermitId,
                        principalTable: "permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permit_medical_exam_renewal_attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitMedicalExamRenewalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_permit_medical_exam_renewal_attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permit_medical_exam_renewal_attachments_permit_medical_exam~",
                        column: x => x.PermitMedicalExamRenewalId,
                        principalTable: "permit_medical_exam_renewals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permit_medical_exam_renewal_attachments_PermitMedicalExamRenewalId_AttachmentType",
                table: "permit_medical_exam_renewal_attachments",
                columns: new[] { "PermitMedicalExamRenewalId", "AttachmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_permit_medical_exam_renewals_CitizenId",
                table: "permit_medical_exam_renewals",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_permit_medical_exam_renewals_PermitId_Pending",
                table: "permit_medical_exam_renewals",
                column: "PermitId",
                unique: true,
                filter: "\"Status\" IN (0, 1)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "permit_medical_exam_renewal_attachments");
            migrationBuilder.DropTable(name: "permit_medical_exam_renewals");
        }
    }
}
