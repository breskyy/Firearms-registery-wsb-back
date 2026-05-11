using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWeaponRegistry.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserRole = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    OldValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    NewValuesJson = table.Column<string>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "citizen_profiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstNameEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    LastNameEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    PeselEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    AddressEncrypted = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    DocumentNumberEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    WeaponBookNumberEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_citizen_profiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_citizen_profiles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LicenseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shops_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "firearms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerCitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Caliber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProductionYear = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_firearms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_firearms_citizen_profiles_OwnerCitizenId",
                        column: x => x.OwnerCitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "permits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PermitType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MaxFirearms = table.Column<int>(type: "integer", nullable: false),
                    UsedSlots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MedicalExamExpiryDateEncrypted = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PsychologicalExamExpiryDateEncrypted = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_permits_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ownership_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirearmId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreviousOwnerCitizenId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewOwnerCitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    TransferDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ownership_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ownership_histories_citizen_profiles_NewOwnerCitizenId",
                        column: x => x.NewOwnerCitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ownership_histories_citizen_profiles_PreviousOwnerCitizenId",
                        column: x => x.PreviousOwnerCitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ownership_histories_firearms_FirearmId",
                        column: x => x.FirearmId,
                        principalTable: "firearms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ownership_histories_users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transfer_requests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirearmId = table.Column<Guid>(type: "uuid", nullable: false),
                    SellerCitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    BuyerCitizenId = table.Column<Guid>(type: "uuid", nullable: true),
                    BuyerPeselEncrypted = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    TransferType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TransactionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transfer_requests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_transfer_requests_citizen_profiles_BuyerCitizenId",
                        column: x => x.BuyerCitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transfer_requests_citizen_profiles_SellerCitizenId",
                        column: x => x.SellerCitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transfer_requests_firearms_FirearmId",
                        column: x => x.FirearmId,
                        principalTable: "firearms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medical_alerts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitId = table.Column<Guid>(type: "uuid", nullable: true),
                    AlertType = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medical_alerts_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_medical_alerts_permits_PermitId",
                        column: x => x.PermitId,
                        principalTable: "permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "promises",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromiseNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    WeaponType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    UsedQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    QrToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promises", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promises_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promises_permits_PermitId",
                        column: x => x.PermitId,
                        principalTable: "permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promise_applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CitizenId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermitId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedWeaponType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RequestedQuantity = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RejectionReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CorrectionNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReviewedByOfficerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GeneratedPromiseId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promise_applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_promise_applications_citizen_profiles_CitizenId",
                        column: x => x.CitizenId,
                        principalTable: "citizen_profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promise_applications_permits_PermitId",
                        column: x => x.PermitId,
                        principalTable: "permits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_promise_applications_promises_GeneratedPromiseId",
                        column: x => x.GeneratedPromiseId,
                        principalTable: "promises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_promise_applications_users_ReviewedByOfficerId",
                        column: x => x.ReviewedByOfficerId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_Action",
                table: "audit_logs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityType_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_TimestampUtc",
                table: "audit_logs",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_citizen_profiles_UserId",
                table: "citizen_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_firearms_OwnerCitizenId",
                table: "firearms",
                column: "OwnerCitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_firearms_SerialNumber",
                table: "firearms",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medical_alerts_CitizenId",
                table: "medical_alerts",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_medical_alerts_PermitId",
                table: "medical_alerts",
                column: "PermitId");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_histories_CreatedByUserId",
                table: "ownership_histories",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_histories_FirearmId",
                table: "ownership_histories",
                column: "FirearmId");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_histories_NewOwnerCitizenId",
                table: "ownership_histories",
                column: "NewOwnerCitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_ownership_histories_PreviousOwnerCitizenId",
                table: "ownership_histories",
                column: "PreviousOwnerCitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_permits_CitizenId",
                table: "permits",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_permits_PermitNumber",
                table: "permits",
                column: "PermitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promise_applications_CitizenId",
                table: "promise_applications",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_promise_applications_GeneratedPromiseId",
                table: "promise_applications",
                column: "GeneratedPromiseId");

            migrationBuilder.CreateIndex(
                name: "IX_promise_applications_PermitId",
                table: "promise_applications",
                column: "PermitId");

            migrationBuilder.CreateIndex(
                name: "IX_promise_applications_ReviewedByOfficerId",
                table: "promise_applications",
                column: "ReviewedByOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_promises_CitizenId",
                table: "promises",
                column: "CitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_promises_PermitId",
                table: "promises",
                column: "PermitId");

            migrationBuilder.CreateIndex(
                name: "IX_promises_PromiseNumber",
                table: "promises",
                column: "PromiseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promises_QrToken",
                table: "promises",
                column: "QrToken");

            migrationBuilder.CreateIndex(
                name: "IX_shops_LicenseNumber",
                table: "shops",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_shops_UserId",
                table: "shops",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_BuyerCitizenId",
                table: "transfer_requests",
                column: "BuyerCitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_FirearmId",
                table: "transfer_requests",
                column: "FirearmId");

            migrationBuilder.CreateIndex(
                name: "IX_transfer_requests_SellerCitizenId",
                table: "transfer_requests",
                column: "SellerCitizenId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "medical_alerts");

            migrationBuilder.DropTable(
                name: "ownership_histories");

            migrationBuilder.DropTable(
                name: "promise_applications");

            migrationBuilder.DropTable(
                name: "shops");

            migrationBuilder.DropTable(
                name: "transfer_requests");

            migrationBuilder.DropTable(
                name: "promises");

            migrationBuilder.DropTable(
                name: "firearms");

            migrationBuilder.DropTable(
                name: "permits");

            migrationBuilder.DropTable(
                name: "citizen_profiles");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
