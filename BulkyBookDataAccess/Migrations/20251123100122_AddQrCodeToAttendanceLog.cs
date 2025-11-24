using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ScannerDataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCodeToAttendanceLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "49697602-ad6c-420b-8b23-fac5cbf3518e");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "930d89c2-a266-4125-b70b-b00e7222f0ce");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "a56420bd-36b0-46d9-94ac-7c151cd88b18");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "d0ac012b-d1a5-4536-9b67-20a131224623");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "dd7f4c8e-007c-43a8-b5cb-6fc245e60539");

            migrationBuilder.AddColumn<string>(
                name: "QrCode",
                table: "AttendanceLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            //migrationBuilder.InsertData(
            //    table: "AspNetRoles",
            //    columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
            //    values: new object[,]
            //    {
            //        { "41547142-4d83-4ac9-952e-c709f84281f6", "5", "Student", "Student" },
            //        { "bd3018d9-7061-444a-8c84-3585757c452b", "1", "HR", "HR" },
            //        { "c344a73b-abed-4023-8cb0-d6666b232570", "2", "Dean", "Dean" },
            //        { "ca02376a-5c42-41b3-be79-7bddef4ada32", "3", "HeadOfDepartment", "HeadOfDepartment" },
            //        { "f537384c-6e91-4e2e-9949-d97950a57e7b", "4", "Docter", "Doctor" }
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "41547142-4d83-4ac9-952e-c709f84281f6");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "bd3018d9-7061-444a-8c84-3585757c452b");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "c344a73b-abed-4023-8cb0-d6666b232570");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "ca02376a-5c42-41b3-be79-7bddef4ada32");

            //migrationBuilder.DeleteData(
            //    table: "AspNetRoles",
            //    keyColumn: "Id",
            //    keyValue: "f537384c-6e91-4e2e-9949-d97950a57e7b");

            migrationBuilder.DropColumn(
                name: "QrCode",
                table: "AttendanceLogs");

            //migrationBuilder.InsertData(
            //    table: "AspNetRoles",
            //    columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
            //    values: new object[,]
            //    {
            //        { "49697602-ad6c-420b-8b23-fac5cbf3518e", "1", "HR", "HR" },
            //        { "930d89c2-a266-4125-b70b-b00e7222f0ce", "2", "Dean", "Dean" },
            //        { "a56420bd-36b0-46d9-94ac-7c151cd88b18", "5", "Student", "Student" },
            //        { "d0ac012b-d1a5-4536-9b67-20a131224623", "3", "HeadOfDepartment", "HeadOfDepartment" },
            //        { "dd7f4c8e-007c-43a8-b5cb-6fc245e60539", "4", "Docter", "Doctor" }
            //    });
        }
    }
}
