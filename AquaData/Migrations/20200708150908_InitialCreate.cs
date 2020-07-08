using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AquaMonitor.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Readings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Value = table.Column<double>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    Scale = table.Column<int>(nullable: false),
                    Taken = table.Column<DateTime>(nullable: false),
                    Location = table.Column<string>(nullable: true),
                    IdealMin = table.Column<double>(nullable: false),
                    IdealMax = table.Column<double>(nullable: false),
                    Note = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Readings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Records",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TempF = table.Column<double>(nullable: false),
                    TempC = table.Column<double>(nullable: false),
                    Humidity = table.Column<double>(nullable: false),
                    OutsideTempF = table.Column<double>(nullable: false),
                    OutsideTempC = table.Column<double>(nullable: false),
                    OutsideHumidity = table.Column<double>(nullable: false),
                    WindSpeed = table.Column<double>(nullable: true),
                    Sunrise = table.Column<DateTime>(nullable: true),
                    Sunset = table.Column<DateTime>(nullable: true),
                    CloudCoverage = table.Column<double>(nullable: true),
                    Rain = table.Column<bool>(nullable: true),
                    SystemRunning = table.Column<bool>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Serialize = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Records", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Relays",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    Letter = table.Column<int>(nullable: false),
                    Interval = table.Column<int>(nullable: false),
                    IntervalRun = table.Column<int>(nullable: false),
                    Start = table.Column<TimeSpan>(nullable: true),
                    Stop = table.Column<TimeSpan>(nullable: true),
                    MinTempF = table.Column<double>(nullable: true),
                    MaxTempF = table.Column<double>(nullable: true),
                    MinOutTempF = table.Column<double>(nullable: true),
                    MaxOutTempF = table.Column<double>(nullable: true),
                    TempVariance = table.Column<double>(nullable: true),
                    CurrentState = table.Column<int>(nullable: false),
                    WaterId = table.Column<int>(nullable: true),
                    OnWhenFloatHigh = table.Column<bool>(nullable: false),
                    Pin = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Relays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TempPin = table.Column<int>(nullable: false),
                    TempType = table.Column<int>(nullable: false),
                    DataCollectionRate = table.Column<int>(nullable: false),
                    AdminPassword = table.Column<string>(nullable: true, defaultValue: "fishy"),
                    Zipcode = table.Column<string>(nullable: true),
                    Country = table.Column<string>(nullable: true),
                    APIKey = table.Column<string>(nullable: true),
                    SettingA = table.Column<string>(nullable: true),
                    SettingB = table.Column<string>(nullable: true),
                    SettingC = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WaterLevels",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(nullable: true),
                    FloatHigh = table.Column<bool>(nullable: false),
                    Pin = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterLevels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerReading",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReaderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    PowerState = table.Column<int>(nullable: false),
                    HistoryRecordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerReading_Records_HistoryRecordId",
                        column: x => x.HistoryRecordId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WaterReading",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReaderId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    WaterLevelHigh = table.Column<bool>(nullable: false),
                    HistoryRecordId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaterReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaterReading_Records_HistoryRecordId",
                        column: x => x.HistoryRecordId,
                        principalTable: "Records",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PowerReading_HistoryRecordId",
                table: "PowerReading",
                column: "HistoryRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_WaterReading_HistoryRecordId",
                table: "WaterReading",
                column: "HistoryRecordId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PowerReading");

            migrationBuilder.DropTable(
                name: "Readings");

            migrationBuilder.DropTable(
                name: "Relays");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "WaterLevels");

            migrationBuilder.DropTable(
                name: "WaterReading");

            migrationBuilder.DropTable(
                name: "Records");
        }
    }
}
