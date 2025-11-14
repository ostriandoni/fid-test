using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fujitsu.Migrations
{
    /// <inheritdoc />
    public partial class InitDatabase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // --- Manually injected code based on the Supplier Model ---
            migrationBuilder.CreateTable(
                name: "Suppliers",
                columns: table => new
                {
                    // Primary Key
                    SupplierId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    
                    // Required string fields
                    SupplierCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SupplierName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    
                    // Optional string fields (assuming no [Required] was applied)
                    Province = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true), 
                    City = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true), 
                    Address = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    ContactPerson = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suppliers", x => x.SupplierId);
                });
            // --------------------------------------------------------
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Manually injected code to drop the table on rollback
            migrationBuilder.DropTable(
                name: "Suppliers");
        }
    }
}