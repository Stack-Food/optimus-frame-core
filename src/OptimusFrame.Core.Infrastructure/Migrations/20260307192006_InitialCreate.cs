using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace OptimusFrame.Core.Infrastructure.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "media",
                columns: table => new
                {
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    base64 = table.Column<string>(type: "text", nullable: false),
                    url_bucket = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_media", x => x.media_id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_media_created_at",
                table: "media",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_media_status",
                table: "media",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_media_username",
                table: "media",
                column: "user_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "media");
        }
    }
}
