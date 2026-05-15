using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIJobHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase3_CoverLetter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemplateMarkdown",
                table: "CoverLetterConfigs",
                newName: "HeaderTemplate");

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "CoverLetters",
                type: "varchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "PdfPath",
                table: "CoverLetters",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "FooterTemplate",
                table: "CoverLetterConfigs",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "CoverLetters");

            migrationBuilder.DropColumn(
                name: "PdfPath",
                table: "CoverLetters");

            migrationBuilder.DropColumn(
                name: "FooterTemplate",
                table: "CoverLetterConfigs");

            migrationBuilder.RenameColumn(
                name: "HeaderTemplate",
                table: "CoverLetterConfigs",
                newName: "TemplateMarkdown");
        }
    }
}
