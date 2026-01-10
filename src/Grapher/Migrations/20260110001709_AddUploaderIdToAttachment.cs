using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grapher.Migrations
{
    /// <inheritdoc />
    public partial class AddUploaderIdToAttachment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "TaskAssignments",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploaderId",
                table: "Attachments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_UploaderId",
                table: "Attachments",
                column: "UploaderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AspNetUsers_UploaderId",
                table: "Attachments",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AspNetUsers_UploaderId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_UploaderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "UploaderId",
                table: "Attachments");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedByUserId",
                table: "TaskAssignments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
