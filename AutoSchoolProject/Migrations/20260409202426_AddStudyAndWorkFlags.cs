using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSchoolProject.Migrations
{
    /// <inheritdoc />
    public partial class AddStudyAndWorkFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Still studying",
                table: "Students",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Is working",
                table: "Instructors",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Still studying",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "Is working",
                table: "Instructors");
        }
    }
}
