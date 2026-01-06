using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grapher.Migrations
{
    /// <inheritdoc />
    public partial class AddParentTaskToTaskItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentTaskId",
                table: "TaskItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskItemId",
                table: "Attachments",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaskItems_ParentTaskId",
                table: "TaskItems",
                column: "ParentTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TaskItemId",
                table: "Attachments",
                column: "TaskItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_TaskItems_TaskItemId",
                table: "Attachments",
                column: "TaskItemId",
                principalTable: "TaskItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TaskItems_TaskItems_ParentTaskId",
                table: "TaskItems",
                column: "ParentTaskId",
                principalTable: "TaskItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskItems_TaskItemId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_TaskItems_TaskItems_ParentTaskId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_TaskItems_ParentTaskId",
                table: "TaskItems");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_TaskItemId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ParentTaskId",
                table: "TaskItems");

            migrationBuilder.DropColumn(
                name: "TaskItemId",
                table: "Attachments");
        }
    }
}
