using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlShortener.Infrastructure.Persistence.Migrations
{
    public partial class AddClickEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClickEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShortUrlId = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    VisitorHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UserAgent = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    DeviceType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Os = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Browser = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    CountryCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClickEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortUrlId",
                table: "ClickEvents",
                column: "ShortUrlId");

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortUrlId_OccurredAt",
                table: "ClickEvents",
                columns: new[] { "ShortUrlId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClickEvents_ShortUrlId_VisitorHash",
                table: "ClickEvents",
                columns: new[] { "ShortUrlId", "VisitorHash" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClickEvents");
        }
    }
}
