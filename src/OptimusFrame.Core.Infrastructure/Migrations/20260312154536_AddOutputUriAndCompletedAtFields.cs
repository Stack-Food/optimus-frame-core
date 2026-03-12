using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OptimusFrame.Core.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutputUriAndCompletedAtFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "media",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "media",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OutputUri",
                table: "media",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "media");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "media");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "media");

            migrationBuilder.DropColumn(
                name: "OutputUri",
                table: "media");
        }
    }
}
