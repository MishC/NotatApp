using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotatApp.Migrations
{
    /// <inheritdoc />
    public partial class OverdueFolderwithindex0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Folders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Overdue", null });

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Work", null });

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Personal", null });

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Name", "UserId" },
                values: new object[] { "Ideas", null });

            migrationBuilder.InsertData(
                table: "Folders",
                columns: new[] { "Id", "Name", "UserId" },
                values: new object[] { 5, "Done", null });

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId",
                table: "Folders",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Folders_AspNetUsers_UserId",
                table: "Folders",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Folders_AspNetUsers_UserId",
                table: "Folders");

            migrationBuilder.DropIndex(
                name: "IX_Folders_UserId",
                table: "Folders");

            migrationBuilder.DeleteData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Folders");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "Work");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "Personal");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "Ideas");

            migrationBuilder.UpdateData(
                table: "Folders",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "Done");
        }
    }
}
