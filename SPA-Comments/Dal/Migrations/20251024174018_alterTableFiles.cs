using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dal.Migrations
{
    /// <inheritdoc />
    public partial class alterTableFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Comments_CommentId",
                schema: "dbo",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileData",
                schema: "dbo",
                table: "Files");

            migrationBuilder.AddColumn<string>(
                name: "FileUri",
                schema: "dbo",
                table: "Files",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Comments_CommentId",
                schema: "dbo",
                table: "Files",
                column: "CommentId",
                principalSchema: "dbo",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Comments_CommentId",
                schema: "dbo",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "FileUri",
                schema: "dbo",
                table: "Files");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                schema: "dbo",
                table: "Files",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Comments_CommentId",
                schema: "dbo",
                table: "Files",
                column: "CommentId",
                principalSchema: "dbo",
                principalTable: "Comments",
                principalColumn: "CommentId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
