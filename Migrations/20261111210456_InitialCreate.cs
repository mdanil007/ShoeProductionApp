using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ShoeProductionApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Models_Colors_ColorId",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Products_Gender_LeatherId_LiningId_ModelId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Gender",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Models_Code_ColorId",
                table: "Models");

            migrationBuilder.DropIndex(
                name: "IX_Models_ColorId",
                table: "Models");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Model_Code",
                table: "Models");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lining_Code",
                table: "Linings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Leather_Code",
                table: "Leathers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Color_Code",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Models");

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SoleId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Models",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Colors",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "Soles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Soles", x => x.Id);
                    table.CheckConstraint("CK_Sole_Code", "\"Code\" ~ '^[0-9]{3}$'");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_ColorId",
                table: "Products",
                column: "ColorId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Gender_ModelId_ColorId_LeatherId_LiningId_SoleId",
                table: "Products",
                columns: new[] { "Gender", "ModelId", "ColorId", "LeatherId", "LiningId", "SoleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_SoleId",
                table: "Products",
                column: "SoleId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Gender",
                table: "Products",
                sql: "\"Gender\" IN (6, 7, 8, 9)");

            migrationBuilder.CreateIndex(
                name: "IX_Models_Code",
                table: "Models",
                column: "Code",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Model_Code",
                table: "Models",
                sql: "\"Code\" ~ '^[0-9]{3}$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lining_Code",
                table: "Linings",
                sql: "\"Code\" ~ '^[0-9]{1}$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Leather_Code",
                table: "Leathers",
                sql: "\"Code\" ~ '^[0-9]{1}$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Color_Code",
                table: "Colors",
                sql: "\"Code\" ~ '^[0-9]{2}$'");

            migrationBuilder.CreateIndex(
                name: "IX_Soles_Code",
                table: "Soles",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Colors_ColorId",
                table: "Products",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Soles_SoleId",
                table: "Products",
                column: "SoleId",
                principalTable: "Soles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Colors_ColorId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Soles_SoleId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Soles");

            migrationBuilder.DropIndex(
                name: "IX_Products_ColorId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_Gender_ModelId_ColorId_LeatherId_LiningId_SoleId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Products_SoleId",
                table: "Products");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Product_Gender",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_Models_Code",
                table: "Models");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Model_Code",
                table: "Models");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Lining_Code",
                table: "Linings");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Leather_Code",
                table: "Leathers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Color_Code",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "ColorId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SoleId",
                table: "Products");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Models",
                type: "character varying(2)",
                maxLength: 2,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<int>(
                name: "ColorId",
                table: "Models",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Colors",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2)",
                oldMaxLength: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Gender_LeatherId_LiningId_ModelId",
                table: "Products",
                columns: new[] { "Gender", "LeatherId", "LiningId", "ModelId" },
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Product_Gender",
                table: "Products",
                sql: "\"Gender\" IN (8, 9)");

            migrationBuilder.CreateIndex(
                name: "IX_Models_Code_ColorId",
                table: "Models",
                columns: new[] { "Code", "ColorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_ColorId",
                table: "Models",
                column: "ColorId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Model_Code",
                table: "Models",
                sql: "\"Code\" ~ '^[0-9]{2}$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Lining_Code",
                table: "Linings",
                sql: "\"Code\" ~ '^[0-9]$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Leather_Code",
                table: "Leathers",
                sql: "\"Code\" ~ '^[0-9]$'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Color_Code",
                table: "Colors",
                sql: "\"Code\" ~ '^[0-9]$'");

            migrationBuilder.AddForeignKey(
                name: "FK_Models_Colors_ColorId",
                table: "Models",
                column: "ColorId",
                principalTable: "Colors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
