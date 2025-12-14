using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grapher.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTaskItemCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_AssigneeId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_AssigneeId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "TaskItems");

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "TaskItems",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_CreatorId",
                table: "TaskItems",
                column: "CreatorId");

            migrationBuilder.Sql("UPDATE \"TaskItems\" SET \"CreatorId\" = (SELECT \"Id\" FROM \"AspNetUsers\" LIMIT 1) WHERE \"CreatorId\" = ''");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatorId",
                table: "TaskItems",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_AspNetUsers_CreatorId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_CreatorId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "TaskItems");

            migrationBuilder.AddColumn<string>(
                name: "AssigneeId",
                table: "TaskItems",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_AssigneeId",
                table: "TaskItems",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_AspNetUsers_AssigneeId",
                table: "TaskItems",
                column: "AssigneeId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
