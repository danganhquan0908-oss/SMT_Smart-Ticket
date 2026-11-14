using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusGoSmartTicket.Data.Migrations
{
    /// <inheritdoc />
    public partial class MergeBusGoMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseAmount",
                table: "Bookings",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "Bookings",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "DiscountCampaignId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountName",
                table: "Bookings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiscountCampaigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Tier = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Percent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    StartsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MemberNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CampaignId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberNotifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberNotifications_DiscountCampaigns_CampaignId",
                        column: x => x.CampaignId,
                        principalTable: "DiscountCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_DiscountCampaignId",
                table: "Bookings",
                column: "DiscountCampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountCampaigns_Tier_IsActive_EndsAtUtc",
                table: "DiscountCampaigns",
                columns: new[] { "Tier", "IsActive", "EndsAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberNotifications_CampaignId",
                table: "MemberNotifications",
                column: "CampaignId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberNotifications_UserId_CampaignId",
                table: "MemberNotifications",
                columns: new[] { "UserId", "CampaignId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_DiscountCampaigns_DiscountCampaignId",
                table: "Bookings",
                column: "DiscountCampaignId",
                principalTable: "DiscountCampaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
            migrationBuilder.Sql("UPDATE \"Bookings\" SET \"BaseAmount\" = \"TotalAmount\" WHERE \"BaseAmount\" = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_DiscountCampaigns_DiscountCampaignId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "MemberNotifications");

            migrationBuilder.DropTable(
                name: "DiscountCampaigns");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_DiscountCampaignId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BaseAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountCampaignId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "DiscountName",
                table: "Bookings");
        }
    }
}
