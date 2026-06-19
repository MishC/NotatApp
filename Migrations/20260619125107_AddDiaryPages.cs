using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotatApp.Migrations
{
    /// <inheritdoc />
    public partial class AddDiaryPages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_UserId",
                table: "DiaryEntries");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DiaryEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DiaryPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DiaryEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    PageNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 20000, nullable: true),
                    ImagePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ImageContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ImageFileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ImageUploadedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiaryPages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiaryPages_DiaryEntries_DiaryEntryId",
                        column: x => x.DiaryEntryId,
                        principalTable: "DiaryEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("""
                INSERT INTO DiaryPages (
                    DiaryEntryId,
                    PageNumber,
                    Content,
                    ImagePath,
                    ImageContentType,
                    ImageFileName,
                    CreatedAt,
                    ImageUploadedAt
                )
                SELECT
                    Id,
                    CASE WHEN PageNumber < 1 THEN 1 ELSE PageNumber END,
                    Content,
                    ImagePath,
                    ImageContentType,
                    ImageFileName,
                    CreatedAt,
                    CASE WHEN ImagePath IS NULL THEN NULL ELSE CreatedAt END
                FROM DiaryEntries
                WHERE Content IS NOT NULL
                   OR ImagePath IS NOT NULL
                   OR ImageContentType IS NOT NULL
                   OR ImageFileName IS NOT NULL
                   OR PageNumber > 0;
                """);

            migrationBuilder.DropColumn(
                name: "Content",
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

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId_Date",
                table: "DiaryEntries",
                columns: new[] { "UserId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DiaryPages_DiaryEntryId_PageNumber",
                table: "DiaryPages",
                columns: new[] { "DiaryEntryId", "PageNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiaryPages");

            migrationBuilder.DropIndex(
                name: "IX_DiaryEntries_UserId_Date",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DiaryEntries");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "DiaryEntries",
                type: "TEXT",
                maxLength: 20000,
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_DiaryEntries_UserId",
                table: "DiaryEntries",
                column: "UserId");
        }
    }
}
