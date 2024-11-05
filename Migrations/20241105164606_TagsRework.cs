using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Creators.Migrations
{
    /// <inheritdoc />
    public partial class TagsRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagInfo");

            migrationBuilder.AlterColumn<string>(
                name: "BlacklistedTagsName",
                table: "UsersBlacklistedTags",
                type: "character varying(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "AliasedToName",
                table: "Tags",
                type: "character varying(40)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsNSFW",
                table: "Tags",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "TagsName",
                table: "PublicationTag",
                type: "character varying(40)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "CategoryTag",
                columns: table => new
                {
                    CategoriesId = table.Column<int>(type: "integer", nullable: false),
                    TagName = table.Column<string>(type: "character varying(40)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryTag", x => new { x.CategoriesId, x.TagName });
                    table.ForeignKey(
                        name: "FK_CategoryTag_Categories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryTag_Tags_TagName",
                        column: x => x.TagName,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tags_AliasedToName",
                table: "Tags",
                column: "AliasedToName");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryTag_TagName",
                table: "CategoryTag",
                column: "TagName");

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Tags_AliasedToName",
                table: "Tags",
                column: "AliasedToName",
                principalTable: "Tags",
                principalColumn: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Tags_AliasedToName",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "CategoryTag");

            migrationBuilder.DropIndex(
                name: "IX_Tags_AliasedToName",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "AliasedToName",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "IsNSFW",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "BlacklistedTagsName",
                table: "UsersBlacklistedTags",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "TagsName",
                table: "PublicationTag",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)");

            migrationBuilder.CreateTable(
                name: "TagInfo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    Aliases = table.Column<List<string>>(type: "text[]", nullable: false),
                    IsNSFW = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TagInfo_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TagInfo_Tags_Id",
                        column: x => x.Id,
                        principalTable: "Tags",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TagInfo_CategoryId",
                table: "TagInfo",
                column: "CategoryId");
        }
    }
}
