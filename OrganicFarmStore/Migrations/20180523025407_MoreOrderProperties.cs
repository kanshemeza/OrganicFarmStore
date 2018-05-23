using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace OrganicFarmStore.Migrations
{
    public partial class MoreOrderProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Products_ProductID",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_ProductID",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "State",
                table: "Orders",
                newName: "TrackingNumber");

            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Orders",
                newName: "Region");

            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "Orders",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "Orders",
                newName: "Locale");

            migrationBuilder.RenameColumn(
                name: "ExpirationDate",
                table: "Orders",
                newName: "OrderDate");

            migrationBuilder.RenameColumn(
                name: "CreditCardNumber",
                table: "Orders",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Orders",
                newName: "AddressLine2");

            migrationBuilder.RenameColumn(
                name: "BillingAddress",
                table: "Orders",
                newName: "AddressLine1");

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "OrderItems",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItems",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductPrice",
                table: "OrderItems",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ProductPrice",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "TrackingNumber",
                table: "Orders",
                newName: "State");

            migrationBuilder.RenameColumn(
                name: "Region",
                table: "Orders",
                newName: "Phone");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Orders",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "OrderDate",
                table: "Orders",
                newName: "ExpirationDate");

            migrationBuilder.RenameColumn(
                name: "Locale",
                table: "Orders",
                newName: "FirstName");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Orders",
                newName: "CreditCardNumber");

            migrationBuilder.RenameColumn(
                name: "AddressLine2",
                table: "Orders",
                newName: "City");

            migrationBuilder.RenameColumn(
                name: "AddressLine1",
                table: "Orders",
                newName: "BillingAddress");

            migrationBuilder.AlterColumn<int>(
                name: "ProductID",
                table: "OrderItems",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ProductID",
                table: "OrderItems",
                column: "ProductID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Products_ProductID",
                table: "OrderItems",
                column: "ProductID",
                principalTable: "Products",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
