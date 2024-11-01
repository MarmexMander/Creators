using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class CaategoryAsClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Categorys",
                table: "TagInfo",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Publications",
                newName: "CategoryId");

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagInfo_CategoryId",
                table: "TagInfo",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Publications_CategoryId",
                table: "Publications",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Publications_Category_CategoryId",
                table: "Publications",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TagInfo_Category_CategoryId",
                table: "TagInfo",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publications_Category_CategoryId",
                table: "Publications");

            migrationBuilder.DropForeignKey(
                name: "FK_TagInfo_Category_CategoryId",
                table: "TagInfo");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropIndex(
                name: "IX_TagInfo_CategoryId",
                table: "TagInfo");

            migrationBuilder.DropIndex(
                name: "IX_Publications_CategoryId",
                table: "Publications");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "TagInfo",
                newName: "Categorys");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Publications",
                newName: "Category");
        }
    }
}
