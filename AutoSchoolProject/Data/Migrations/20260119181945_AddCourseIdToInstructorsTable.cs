using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoSchoolProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseIdToInstructorsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Instructors",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instructors_CourseId",
                table: "Instructors",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Instructors_Courses_CourseId",
                table: "Instructors",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Instructors_Courses_CourseId",
                table: "Instructors");

            migrationBuilder.DropIndex(
                name: "IX_Instructors_CourseId",
                table: "Instructors");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Instructors");
        }
    }
}
