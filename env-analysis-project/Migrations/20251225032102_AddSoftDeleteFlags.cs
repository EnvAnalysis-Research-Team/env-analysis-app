using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace env_analysis_project.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "SourceType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Parameter",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "EmissionSource",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "SourceType");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Parameter");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "EmissionSource");
        }
    }
}
