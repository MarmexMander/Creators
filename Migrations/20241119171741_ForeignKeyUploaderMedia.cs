using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class ForeignKeyUploaderMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Medias_MediaGroup_GroupId",
                table: "Medias");

            migrationBuilder.DropTable(
                name: "MediaGroup");

            migrationBuilder.DropIndex(
                name: "IX_Medias_GroupId",
                table: "Medias");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "Medias");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "Medias",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MediaGroup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaGroup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Medias_GroupId",
                table: "Medias",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_Medias_MediaGroup_GroupId",
                table: "Medias",
                column: "GroupId",
                principalTable: "MediaGroup",
                principalColumn: "Id");
        }
    }
}
