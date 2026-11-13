using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTicket.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Epic6CustomerPortalAndAuthTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerUserId",
                table: "Bookings",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "character varying(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CustomerUserId_CreatedAtUtc",
                table: "Bookings",
                columns: new[] { "CustomerUserId", "CreatedAtUtc" });

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_CustomerUserId",
                table: "Bookings",
                column: "CustomerUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_CustomerUserId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CustomerUserId_CreatedAtUtc",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CustomerUserId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");
        }
    }
}
