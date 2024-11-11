using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeparateUploaderTier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Medias_PreviewGuid",
                table: "Publications");

            migrationBuilder.DropIndex(
                name: "IX_Publications_PreviewGuid",
                table: "Publications");

            migrationBuilder.DropColumn(
                name: "PreviewGuid",
                table: "Publications");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Publications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "UploadTier",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploadTier", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "UploadTier",
                column: "Id",
                values: new object[]
                {
                    1,
                    2,
                    3,
                    4,
                    5
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UploadTier");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Publications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<Guid>(
                name: "PreviewGuid",
                table: "Publications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Publications_PreviewGuid",
                table: "Publications",
                column: "PreviewGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Medias_PreviewGuid",
                table: "Publications",
                column: "PreviewGuid",
                principalTable: "Medias",
                principalColumn: "Guid",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
