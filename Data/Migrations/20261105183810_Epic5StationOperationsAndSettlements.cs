using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusGoSmartTicket.Data.Migrations
{
    /// <inheritdoc />
    public partial class Epic5StationOperationsAndSettlements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets");

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckedInAtUtc",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CheckedInByUserId",
                table: "Tickets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutAccountName",
                table: "BusOperators",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutAccountNumber",
                table: "BusOperators",
                type: "character varying(28)",
                maxLength: 28,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutBankBin",
                table: "BusOperators",
                type: "character varying(6)",
                maxLength: 6,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DispatchClearances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    VehicleDocumentsValid = table.Column<bool>(type: "boolean", nullable: false),
                    DriverLicenseValid = table.Column<bool>(type: "boolean", nullable: false),
                    DriverFitForDuty = table.Column<bool>(type: "boolean", nullable: false),
                    PassengerCapacityValid = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    Signature = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchClearances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DispatchClearances_AspNetUsers_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchClearances_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DispatchClearances_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StationMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StationMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StationMemberships_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StationMemberships_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketScanLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StationId = table.Column<Guid>(type: "uuid", nullable: true),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsOffline = table.Column<bool>(type: "boolean", nullable: false),
                    ScannedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketScanLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketScanLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketScanLogs_Stations_StationId",
                        column: x => x.StationId,
                        principalTable: "Stations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketScanLogs_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TripSettlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TripId = table.Column<Guid>(type: "uuid", nullable: false),
                    OperatorId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: true),
                    FailureReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessingAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TripSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TripSettlements_BusOperators_OperatorId",
                        column: x => x.OperatorId,
                        principalTable: "BusOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TripSettlements_Trips_TripId",
                        column: x => x.TripId,
                        principalTable: "Trips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets",
                column: "SeatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DispatchClearances_ReviewedByUserId",
                table: "DispatchClearances",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchClearances_StationId",
                table: "DispatchClearances",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchClearances_TripId_CreatedAtUtc",
                table: "DispatchClearances",
                columns: new[] { "TripId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_StationMemberships_StationId_UserId",
                table: "StationMemberships",
                columns: new[] { "StationId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StationMemberships_UserId",
                table: "StationMemberships",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketScanLogs_StationId",
                table: "TicketScanLogs",
                column: "StationId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketScanLogs_TicketId_ScannedAtUtc",
                table: "TicketScanLogs",
                columns: new[] { "TicketId", "ScannedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketScanLogs_UserId",
                table: "TicketScanLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TripSettlements_OperatorId",
                table: "TripSettlements",
                column: "OperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_TripSettlements_Status_ProcessingAtUtc",
                table: "TripSettlements",
                columns: new[] { "Status", "ProcessingAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_TripSettlements_TripId",
                table: "TripSettlements",
                column: "TripId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchClearances");

            migrationBuilder.DropTable(
                name: "StationMemberships");

            migrationBuilder.DropTable(
                name: "TicketScanLogs");

            migrationBuilder.DropTable(
                name: "TripSettlements");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CheckedInAtUtc",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CheckedInByUserId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "PayoutAccountName",
                table: "BusOperators");

            migrationBuilder.DropColumn(
                name: "PayoutAccountNumber",
                table: "BusOperators");

            migrationBuilder.DropColumn(
                name: "PayoutBankBin",
                table: "BusOperators");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SeatId",
                table: "Tickets",
                column: "SeatId");
        }
    }
}
