using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PalletOptimization.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SpecialPallet",
                table: "Elements",
                newName: "IsSpecial");

            migrationBuilder.RenameColumn(
                name: "RotationOptions",
                table: "Elements",
                newName: "RotationRules");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "PalletGroups",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "HeightWidthFactor",
                table: "Elements",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstanceId",
                table: "Elements",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Elements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "PalletGroups");

            migrationBuilder.DropColumn(
                name: "HeightWidthFactor",
                table: "Elements");

            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "Elements");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Elements");

            migrationBuilder.RenameColumn(
                name: "RotationRules",
                table: "Elements",
                newName: "RotationOptions");

            migrationBuilder.RenameColumn(
                name: "IsSpecial",
                table: "Elements",
                newName: "SpecialPallet");
        }
    }
}
