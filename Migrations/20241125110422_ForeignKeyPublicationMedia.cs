using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyPublicationMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Medias_MediaContentGuid",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "MediaContentGuid",
                table: "Publications",
                newName: "MediaContentId");

            migrationBuilder.RenameIndex(
                name: "IX_Publications_MediaContentGuid",
                table: "Publications",
                newName: "IX_Publications_MediaContentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Medias_MediaContentId",
                table: "Publications",
                column: "MediaContentId",
                principalTable: "Medias",
                principalColumn: "Guid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Medias_MediaContentId",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "MediaContentId",
                table: "Publications",
                newName: "MediaContentGuid");

            migrationBuilder.RenameIndex(
                name: "IX_Publications_MediaContentId",
                table: "Publications",
                newName: "IX_Publications_MediaContentGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Medias_MediaContentGuid",
                table: "Publications",
                column: "MediaContentGuid",
                principalTable: "Medias",
                principalColumn: "Guid");
        }
    }
}
