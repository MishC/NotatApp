using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddPageNumberToDiaryEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "CreatedAt",
                table: "DiaryEntries",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<string>(
                name: "ImageContentType",
                table: "DiaryEntries",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "DiaryEntries",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "DiaryEntries",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageNumber",
                table: "DiaryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "ImageContentType",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "PageNumber",
                table: "DiaryEntries");
        }
    }
}
