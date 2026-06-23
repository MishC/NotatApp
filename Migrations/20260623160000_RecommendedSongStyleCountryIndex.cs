using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotatApp.Migrations
{
    /// <inheritdoc />
    [Migration("20260623160000_RecommendedSongStyleCountryIndex")]
    public partial class RecommendedSongStyleCountryIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecommendedSongs_DiaryEntryId",
                table: "RecommendedSongs");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedSongs_DiaryEntryId_Style_Country",
                table: "RecommendedSongs",
                columns: new[] { "DiaryEntryId", "Style", "Country" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecommendedSongs_DiaryEntryId_Style_Country",
                table: "RecommendedSongs");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendedSongs_DiaryEntryId",
                table: "RecommendedSongs",
                column: "DiaryEntryId",
                unique: true);
        }
    }
}
