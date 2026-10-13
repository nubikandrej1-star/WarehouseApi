using Microsoft.EntityFrameworkCore.Migrations;
using WarehouseApi.Data;

#nullable disable

namespace WarehouseApi.Migrations;

public partial class AddShipmentsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Shipments",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                TrackingNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RecipientName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                DestinationAddress = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Created"),
                ProductId = table.Column<int>(type: "int", nullable: false),
                Quantity = table.Column<int>(type: "int", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Shipments", shipment => shipment.Id);
                table.ForeignKey(
                    name: "FK_Shipments_Products_ProductId",
                    column: shipment => shipment.ProductId,
                    principalTable: "Products",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Shipments_ProductId",
            table: "Shipments",
            column: "ProductId");

        migrationBuilder.CreateIndex(
            name: "IX_Shipments_TrackingNumber",
            table: "Shipments",
            column: "TrackingNumber",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Shipments");
    }
}
