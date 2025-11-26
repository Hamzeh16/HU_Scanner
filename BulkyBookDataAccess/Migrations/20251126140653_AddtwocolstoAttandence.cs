using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScannerDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddtwocolstoAttandence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<string>(
                name: "ExcuseDocumentPath",
                table: "AttendanceLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExcused",
                table: "AttendanceLogs",
                type: "bit",
                nullable: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.DropColumn(
                name: "ExcuseDocumentPath",
                table: "AttendanceLogs");

            migrationBuilder.DropColumn(
                name: "IsExcused",
                table: "AttendanceLogs");
          
        }
    }
}
