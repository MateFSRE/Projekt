using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeGames.Data.Migrations
{
    /// <inheritdoc />
    public partial class DailyChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInDailyPool",
                table: "Challenges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDaily",
                table: "Attempts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DailyCompletions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    AttemptId = table.Column<int>(type: "int", nullable: false),
                    PointsAwarded = table.Column<int>(type: "int", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyCompletions_Attempts_AttemptId",
                        column: x => x.AttemptId,
                        principalTable: "Attempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyCompletions_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyCompletions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DailySelections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ChallengeId = table.Column<int>(type: "int", nullable: false),
                    Slot = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailySelections", x => x.Id);
                    table.CheckConstraint("CK_DailySelection_Slot", "[Slot] >= 1 AND [Slot] <= 3");
                    table.ForeignKey(
                        name: "FK_DailySelections_Challenges_ChallengeId",
                        column: x => x.ChallengeId,
                        principalTable: "Challenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    BadgeKey = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserBadges_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 1,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 2,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 3,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 4,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 5,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 6,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 7,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 8,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 9,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 10,
                column: "IsInDailyPool",
                value: true);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 11,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 12,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 13,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 14,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 15,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 16,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 17,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 18,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 19,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 20,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 21,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 22,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 23,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 24,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 25,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 26,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 27,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 28,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 29,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 30,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 31,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 32,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 33,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 34,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 35,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 36,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 37,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 38,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 39,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 40,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 41,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 42,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 43,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 44,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.UpdateData(
                table: "Challenges",
                keyColumn: "Id",
                keyValue: 45,
                column: "IsInDailyPool",
                value: false);

            migrationBuilder.CreateIndex(
                name: "IX_DailyCompletions_AttemptId",
                table: "DailyCompletions",
                column: "AttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyCompletions_ChallengeId",
                table: "DailyCompletions",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailyCompletions_UserId_Date_ChallengeId",
                table: "DailyCompletions",
                columns: new[] { "UserId", "Date", "ChallengeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySelections_ChallengeId",
                table: "DailySelections",
                column: "ChallengeId");

            migrationBuilder.CreateIndex(
                name: "IX_DailySelections_Date_ChallengeId",
                table: "DailySelections",
                columns: new[] { "Date", "ChallengeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailySelections_Date_Slot",
                table: "DailySelections",
                columns: new[] { "Date", "Slot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBadges_UserId_BadgeKey",
                table: "UserBadges",
                columns: new[] { "UserId", "BadgeKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyCompletions");

            migrationBuilder.DropTable(
                name: "DailySelections");

            migrationBuilder.DropTable(
                name: "UserBadges");

            migrationBuilder.DropColumn(
                name: "IsInDailyPool",
                table: "Challenges");

            migrationBuilder.DropColumn(
                name: "IsDaily",
                table: "Attempts");
        }
    }
}
