using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using WarehouseApi.Data;

#nullable disable

namespace WarehouseApi.Migrations;

public partial class AddSuppliersTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "Reason",
            table: "StockTransactions",
            type: "nvarchar(1000)",
            maxLength: 1000,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(250)",
            oldMaxLength: 250);

        migrationBuilder.CreateTable(
            name: "Suppliers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                EdrpouCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Suppliers", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Suppliers_EdrpouCode",
            table: "Suppliers",
            column: "EdrpouCode",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Suppliers");

        migrationBuilder.AlterColumn<string>(
            name: "Reason",
            table: "StockTransactions",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(1000)",
            oldMaxLength: 1000);
    }
}
