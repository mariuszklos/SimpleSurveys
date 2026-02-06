using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSurveys.Api.Migrations
{
    /// <inheritdoc />
    public partial class MoveOptionTypeToSurveyAndDateOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionType",
                table: "SurveyOptions");

            migrationBuilder.AddColumn<string>(
                name: "OptionType",
                table: "Surveys",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OptionType",
                table: "Surveys");

            migrationBuilder.AddColumn<string>(
                name: "OptionType",
                table: "SurveyOptions",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
