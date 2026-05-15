using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIJobHelper.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_Ats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AtsChanges",
                table: "AtsResults",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtsChanges",
                table: "AtsResults");
        }
    }
}
