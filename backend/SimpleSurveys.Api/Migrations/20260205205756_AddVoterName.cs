using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SimpleSurveys.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddVoterName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VoterName",
                table: "Votes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VoterName",
                table: "Votes");
        }
    }
}
