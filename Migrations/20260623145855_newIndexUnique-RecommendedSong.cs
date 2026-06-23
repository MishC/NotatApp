using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotatApp.Migrations
{
    /// <inheritdoc />
    public partial class newIndexUniqueRecommendedSong : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecommendedSongs_DiaryEntries_NotatApp.Models.RecommendedSong_DiaryEntryId",
                table: "RecommendedSongs");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_DiaryEntries_TempId_TempId1",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "NotatApp.Models.RecommendedSong",
                table: "RecommendedSongs");

            migrationBuilder.DropColumn(
                name: "TempId",
                table: "DiaryEntries");

            migrationBuilder.DropColumn(
                name: "TempId1",
                table: "DiaryEntries");

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendedSongs_DiaryEntries_DiaryEntryId",
                table: "RecommendedSongs",
                column: "DiaryEntryId",
                principalTable: "DiaryEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecommendedSongs_DiaryEntries_DiaryEntryId",
                table: "RecommendedSongs");

            migrationBuilder.AddColumn<int>(
                name: "NotatApp.Models.RecommendedSong",
                table: "RecommendedSongs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TempId",
                table: "DiaryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TempId1",
                table: "DiaryEntries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_DiaryEntries_TempId_TempId1",
                table: "DiaryEntries",
                columns: new[] { "TempId", "TempId1" });

            migrationBuilder.AddForeignKey(
                name: "FK_RecommendedSongs_DiaryEntries_NotatApp.Models.RecommendedSong_DiaryEntryId",
                table: "RecommendedSongs",
                columns: new[] { "NotatApp.Models.RecommendedSong", "DiaryEntryId" },
                principalTable: "DiaryEntries",
                principalColumns: new[] { "TempId", "TempId1" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
