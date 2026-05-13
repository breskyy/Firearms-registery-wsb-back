using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWeaponRegistry.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPermitApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "permit_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedPermitType = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    MedicalExamExpiryDateEncrypted = table.Column<string>(type: "text", nullable: true),
                    PsychologicalExamExpiryDateEncrypted = table.Column<string>(type: "text", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedByOfficerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedPermitId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permit_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permit_applications_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_permit_applications_permits_GeneratedPermitId",
                        column: x => x.GeneratedPermitId,
                        principalTable: "permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_permit_applications_users_ReviewedByOfficerId",
                        column: x => x.ReviewedByOfficerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_permit_applications_CitizenId",
                table: "permit_applications",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_permit_applications_GeneratedPermitId",
                table: "permit_applications",
                column: "GeneratedPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_permit_applications_ReviewedByOfficerId",
                table: "permit_applications",
                column: "ReviewedByOfficerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permit_applications");
        }
    }
}
