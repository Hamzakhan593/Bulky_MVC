using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Bulky.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class seedingDataIntoCompanyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Companys",
                columns: new[] { "companyId", "companyCity", "companyName", "companyPostalCode", "companyState", "companyStreetAddress", "companyphoneNumber" },
                values: new object[,]
                {
                    { 1, "sanghar", "company1", "psc123", "sindh", "11 number", "03402696208" },
                    { 2, "sanghar", "company2", "psc123", "sindh", "12 number", "03402696208" },
                    { 3, "sanghar", "company3", "psc123", "sindh", "13 number", "03402696208" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Companys",
                keyColumn: "companyId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Companys",
                keyColumn: "companyId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Companys",
                keyColumn: "companyId",
                keyValue: 3);
        }
    }
}
