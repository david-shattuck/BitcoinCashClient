using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitcoinCash.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BchTransaction",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentType = table.Column<int>(type: "int", nullable: false),
                    FromType = table.Column<int>(type: "int", nullable: false),
                    FromID = table.Column<int>(type: "int", nullable: true),
                    ToType = table.Column<int>(type: "int", nullable: false),
                    ToID = table.Column<int>(type: "int", nullable: true),
                    TxId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BchAmount = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    BchPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BchTransaction", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Key",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrivateKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RemainingCalls = table.Column<int>(type: "int", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Key", x => x.ID)
                        .Annotation("SqlServer:Clustered", false);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Key_Address",
                table: "Key",
                column: "Address")
                .Annotation("SqlServer:Clustered", true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BchTransaction");

            migrationBuilder.DropTable(
                name: "Key");
        }
    }
}
