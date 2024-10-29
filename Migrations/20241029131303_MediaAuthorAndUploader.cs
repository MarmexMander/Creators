using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class MediaAuthorAndUploader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_AspNetUsers_AuthorId",
                table: "Medias");

            migrationBuilder.RenameColumn(
                name: "AuthorId",
                table: "Medias",
                newName: "UploaderId");

            migrationBuilder.RenameIndex(
                name: "IX_Medias_AuthorId",
                table: "Medias",
                newName: "IX_Medias_UploaderId");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Medias",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_AspNetUsers_UploaderId",
                table: "Medias",
                column: "UploaderId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_AspNetUsers_UploaderId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "Author",
                table: "Medias");

            migrationBuilder.RenameColumn(
                name: "UploaderId",
                table: "Medias",
                newName: "AuthorId");

            migrationBuilder.RenameIndex(
                name: "IX_Medias_UploaderId",
                table: "Medias",
                newName: "IX_Medias_AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_AspNetUsers_AuthorId",
                table: "Medias",
                column: "AuthorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
