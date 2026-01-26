using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSchoolProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModelsEnumsLessonStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PracticeLessons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PracticeLessons");
        }
    }
}
