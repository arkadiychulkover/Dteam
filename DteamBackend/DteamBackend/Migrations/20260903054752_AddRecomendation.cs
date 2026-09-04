using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DteamBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRecomendation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordSalt = table.Column<string>(type: "TEXT", nullable: false),
                    WalletAddress = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    PasswordResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetTokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    BalanceInNanoTons = table.Column<long>(type: "INTEGER", nullable: false),
                    TotalEarningsInNanoTons = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsInFamily = table.Column<bool>(type: "INTEGER", nullable: false),
                    FamilyOwnerId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsBanned = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    BannerUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TasteVectorJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Users_FamilyOwnerId",
                        column: x => x.FamilyOwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "FriendRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SenderId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ReceiverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FriendRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FriendRequests_Users_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FriendRequests_Users_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    ShortDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PriceInNanoTons = table.Column<long>(type: "INTEGER", nullable: false),
                    DiscountPercentage = table.Column<int>(type: "INTEGER", nullable: false),
                    ServerArchivePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DownloadCount = table.Column<long>(type: "INTEGER", nullable: false),
                    AverageRating = table.Column<double>(type: "REAL", nullable: false),
                    ReviewsCount = table.Column<int>(type: "INTEGER", nullable: false),
                    IsDlc = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParentGameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Genres = table.Column<string>(type: "TEXT", nullable: false),
                    Platforms = table.Column<string>(type: "TEXT", nullable: false),
                    Features = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    SizeInBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    IsPublished = table.Column<bool>(type: "INTEGER", nullable: false),
                    HeaderImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CoverImageUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ScreenshotUrls = table.Column<string>(type: "TEXT", nullable: false),
                    TrailerUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TasteVectorJson = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Games_ParentGameId",
                        column: x => x.ParentGameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Games_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tranxactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TxhHash = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Amount = table.Column<decimal>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tranxactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tranxactions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserFriends",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FriendId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFriends", x => new { x.UserId, x.FriendId });
                    table.ForeignKey(
                        name: "FK_UserFriends_Users_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFriends_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Rating = table.Column<int>(type: "INTEGER", nullable: false),
                    Content = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    IsRecommended = table.Column<bool>(type: "INTEGER", nullable: false),
                    PlayTimeHoursAtReview = table.Column<double>(type: "REAL", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCartItems",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCartItems", x => new { x.UserId, x.GameId });
                    table.ForeignKey(
                        name: "FK_UserCartItems_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCartItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserGames",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PurchasedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PlayTimeMinutes = table.Column<long>(type: "INTEGER", nullable: false),
                    LastPlayedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsFavorite = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserGames", x => new { x.UserId, x.GameId });
                    table.ForeignKey(
                        name: "FK_UserGames_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserGames_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserWishlists",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GameId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWishlists", x => new { x.UserId, x.GameId });
                    table.ForeignKey(
                        name: "FK_UserWishlists_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWishlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "AvatarUrl", "BalanceInNanoTons", "BannerUrl", "Bio", "CreatedAt", "Email", "FamilyOwnerId", "IsAdmin", "IsBanned", "IsInFamily", "LastLoginAt", "PasswordHash", "PasswordResetToken", "PasswordResetTokenExpiresAt", "PasswordSalt", "Status", "TasteVectorJson", "TotalEarningsInNanoTons", "UpdatedAt", "Username", "WalletAddress" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=500&auto=format&fit=crop&q=60", 100000000000L, null, "Dteam System Administrator", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@gmail.com", null, true, false, false, null, "xzDffecwqtJOD18Zo7UYbldnPXtDzynoJUR7KTJS9ehHADOx/esK2rHhSwpxfCTzLyMtx9ibEjG4nTBZofSSXQ==", null, null, "wgTdAbuN2TajNJ6gq/38JImUkkU+Up8jm6kR08R4+5z8FdWLts0sw5FK/NG6p90Akzkk9uj4zgpWF6m/QRvJ1jNAdUCHknFsyL1B7TC2HcZPEK86BO0fbPixRHjNgI752RyrhH8pHtmHEFlKLzqDxzszAU696vHX4KEn2v/3uLM=", 1, "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", 0L, null, "admin", "EQB_v2zX3L1f2M9zX_SampleAdminTonWalletAddress_777" });

            migrationBuilder.InsertData(
                table: "Games",
                columns: new[] { "Id", "AverageRating", "CoverImageUrl", "CreatedAt", "Description", "DiscountPercentage", "DownloadCount", "Features", "Genres", "HeaderImageUrl", "IsDlc", "IsPublished", "OwnerId", "ParentGameId", "Platforms", "PriceInNanoTons", "ReviewsCount", "ScreenshotUrls", "ServerArchivePath", "ShortDescription", "SizeInBytes", "Tags", "TasteVectorJson", "Title", "TrailerUrl", "UpdatedAt", "Version" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), 4.9000000000000004, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Епічна RPG про відьмака Ґеральта у відкритому світі.", 50, 195347L, "[0]", "[1,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 25000000000L, 4397, "[]", "/games/the-witcher-3-wild-hunt.zip", "Епічна RPG про відьмака Ґеральта у відкритому світі.", 52613349376L, "[\"Open World\",\"Story Rich\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "The Witcher 3: Wild Hunt", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111102"), 3.5, null, new DateTime(2024, 1, 10, 0, 0, 0, 0, DateTimeKind.Utc), "Відкритий світ Найт-Сіті у неоновому кіберпанк-майбутньому.", 10, 2379L, "[0]", "[1,0,4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 30000000000L, 1429, "[]", "/games/cyberpunk-2077.zip", "Відкритий світ Найт-Сіті у неоновому кіберпанк-майбутньому.", 44023414784L, "[\"Open World\",\"Sci-Fi\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Cyberpunk 2077", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111103"), 4.5999999999999996, null, new DateTime(2023, 11, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Похмура фентезійна RPG від FromSoftware з відкритим світом.", 20, 40498L, "[0]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 35000000000L, 4548, "[]", "/games/elden-ring.zip", "Похмура фентезійна RPG від FromSoftware з відкритим світом.", 42949672960L, "[\"Open World\",\"Difficult\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Elden Ring", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111104"), 3.2999999999999998, null, new DateTime(2023, 12, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Складна екшн-RPG у похмурому фентезійному світі.", 0, 25831L, "[0]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 4881, "[]", "/games/dark-souls-iii.zip", "Складна екшн-RPG у похмурому фентезійному світі.", 35433480192L, "[\"Difficult\",\"Fantasy\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Dark Souls III", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111105"), 4.4000000000000004, null, new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Атмосферний метроідванія у підземному королівстві комах.", 10, 60998L, "[0]", "[3,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 8000000000L, 5048, "[]", "/games/hollow-knight.zip", "Атмосферний метроідванія у підземному королівстві комах.", 21474836480L, "[\"Atmospheric\",\"Pixel Art\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Hollow Knight", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111106"), 3.2999999999999998, null, new DateTime(2024, 5, 9, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(7657), "Затишний фермерський симулятор з елементами RPG.", 0, 82147L, "[0,1]", "[6,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 6000000000L, 1197, "[]", "/games/stardew-valley.zip", "Затишний фермерський симулятор з елементами RPG.", 9663676416L, "[\"Pixel Art\",\"Casual\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Stardew Valley", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111107"), 4.2000000000000002, null, new DateTime(2025, 10, 4, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(7854), "2D пісочниця з виживанням, будівництвом та босами.", 0, 63234L, "[2]", "[6,6,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 5000000000L, 2284, "[]", "/games/terraria.zip", "2D пісочниця з виживанням, будівництвом та босами.", 38654705664L, "[\"Pixel Art\",\"Multiplayer\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Terraria", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111108"), 3.7000000000000002, null, new DateTime(2024, 12, 25, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(7903), "Легендарна пісочниця для будівництва та виживання.", 20, 158517L, "[2]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 15000000000L, 2567, "[]", "/games/minecraft.zip", "Легендарна пісочниця для будівництва та виживання.", 20401094656L, "[\"Open World\",\"Multiplayer\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Minecraft", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111109"), 3.6000000000000001, null, new DateTime(2026, 8, 12, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(7958), "Культова головоломка від першої особи з порталами.", 15, 14022L, "[0,2]", "[5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 7000000000L, 3072, "[]", "/games/portal-2.zip", "Культова головоломка від першої особи з порталами.", 25769803776L, "[\"Story Rich\",\"Funny\",\"Co-op\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Portal 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111110"), 3.6000000000000001, null, new DateTime(2025, 10, 10, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(7996), "Класичний шутер від першої особи, що змінив жанр.", 15, 104528L, "[0]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 5000000000L, 3578, "[]", "/games/half-life-2.zip", "Класичний шутер від першої особи, що змінив жанр.", 32212254720L, "[\"Story Rich\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Half-Life 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111111"), 4.7000000000000002, null, new DateTime(2025, 5, 26, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8062), "Тактичний командний шутер від першої особи.", 25, 116765L, "[1]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,2]", 0L, 815, "[]", "/games/counter-strike-2.zip", "Тактичний командний шутер від першої особи.", 50465865728L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Counter-Strike 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111112"), 4.4000000000000004, null, new DateTime(2024, 9, 19, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8107), "Командна MOBA з десятками унікальних героїв.", 10, 44114L, "[1]", "[2,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 0L, 3164, "[]", "/games/dota-2.zip", "Командна MOBA з десятками унікальних героїв.", 60129542144L, "[\"Multiplayer\",\"Free to Play\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Dota 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111113"), 4.0, null, new DateTime(2026, 1, 4, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8184), "Відкритий світ злочинного Лос-Сантоса.", 50, 43842L, "[0,1]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 2892, "[]", "/games/grand-theft-auto-v.zip", "Відкритий світ злочинного Лос-Сантоса.", 4294967296L, "[\"Open World\",\"Multiplayer\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Grand Theft Auto V", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111114"), 4.5999999999999996, null, new DateTime(2026, 5, 4, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8226), "Епічний вестерн у відкритому світі.", 20, 129522L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 30000000000L, 3572, "[]", "/games/red-dead-redemption-2.zip", "Епічний вестерн у відкритому світі.", 4294967296L, "[\"Open World\",\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Red Dead Redemption 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111115"), 4.2000000000000002, null, new DateTime(2026, 5, 8, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8291), "Легендарна фентезійна RPG з відкритим світом.", 0, 150618L, "[0]", "[1,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 15000000000L, 4668, "[]", "/games/the-elder-scrolls-v-skyrim.zip", "Легендарна фентезійна RPG з відкритим світом.", 64424509440L, "[\"Open World\",\"Fantasy\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "The Elder Scrolls V: Skyrim", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111116"), 4.9000000000000004, null, new DateTime(2024, 9, 14, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8335), "Постапокаліптична RPG у відкритому світі Бостона.", 50, 8719L, "[0]", "[1,4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 15000000000L, 2769, "[]", "/games/fallout-4.zip", "Постапокаліптична RPG у відкритому світі Бостона.", 65498251264L, "[\"Open World\",\"Post-Apocalyptic\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Fallout 4", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111117"), 3.7999999999999998, null, new DateTime(2025, 5, 17, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8484), "Шалено динамічний шутер від першої особи проти демонів.", 25, 79374L, "[0]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 3424, "[]", "/games/doom-eternal.zip", "Шалено динамічний шутер від першої особи проти демонів.", 60129542144L, "[\"Difficult\",\"Sci-Fi\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "DOOM Eternal", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111118"), 3.6000000000000001, null, new DateTime(2024, 11, 20, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8527), "Складна екшн-гра про самурая-шинобі.", 15, 168752L, "[0]", "[0,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 22000000000L, 2802, "[]", "/games/sekiro-shadows-die-twice.zip", "Складна екшн-гра про самурая-шинобі.", 57982058496L, "[\"Difficult\",\"Atmospheric\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Sekiro: Shadows Die Twice", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111119"), 4.0999999999999996, null, new DateTime(2024, 11, 15, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8567), "Пронизливий піксельний платформер про підйом на гору.", 0, 129457L, "[0]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 6000000000L, 3507, "[]", "/games/celeste.zip", "Пронизливий піксельний платформер про підйом на гору.", 63350767616L, "[\"Pixel Art\",\"Difficult\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Celeste", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111120"), 4.2999999999999998, null, new DateTime(2024, 7, 10, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8635), "Рогалик про втечу з підземного царства Аїда.", 0, 83585L, "[0]", "[5,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 12000000000L, 2635, "[]", "/games/hades.zip", "Рогалик про втечу з підземного царства Аїда.", 7516192768L, "[\"Story Rich\",\"Fantasy\",\"Great Soundtrack\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Hades", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111121"), 4.0999999999999996, null, new DateTime(2026, 6, 14, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8677), "Динамічний рогалик-метроідванія з піксельною графікою.", 0, 51581L, "[0]", "[5,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 10000000000L, 631, "[]", "/games/dead-cells.zip", "Динамічний рогалик-метроідванія з піксельною графікою.", 24696061952L, "[\"Pixel Art\",\"Difficult\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Dead Cells", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111122"), 4.0999999999999996, null, new DateTime(2026, 1, 21, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8719), "Карткова гра-рогалик про підйом на вежу.", 0, 142025L, "[0]", "[2,5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 8000000000L, 1075, "[]", "/games/slay-the-spire.zip", "Карткова гра-рогалик про підйом на вежу.", 50465865728L, "[\"Turn-Based\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Slay the Spire", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111123"), 3.7000000000000002, null, new DateTime(2025, 9, 3, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8762), "Покерний рогалик, що затягує на години.", 20, 74565L, "[0]", "[2,5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 5000000000L, 3615, "[]", "/games/balatro.zip", "Покерний рогалик, що затягує на години.", 7516192768L, "[\"Turn-Based\",\"Casual\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Balatro", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111124"), 4.0, null, new DateTime(2025, 2, 14, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8923), "Покрокова глобальна стратегія про розвиток цивілізації.", 50, 119466L, "[0,1]", "[2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 20000000000L, 3516, "[]", "/games/civilization-vi.zip", "Покрокова глобальна стратегія про розвиток цивілізації.", 30064771072L, "[\"Turn-Based\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Civilization VI", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111125"), 3.7000000000000002, null, new DateTime(2024, 4, 17, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(8974), "Класична стратегія реального часу в історичних декораціях.", 20, 185969L, "[0,1]", "[2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 12000000000L, 5019, "[]", "/games/age-of-empires-ii.zip", "Класична стратегія реального часу в історичних декораціях.", 33285996544L, "[\"Real-Time\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Age of Empires II", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111126"), 4.5999999999999996, null, new DateTime(2025, 9, 30, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9014), "Легендарна космічна стратегія реального часу.", 20, 64538L, "[1]", "[2,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 0L, 3588, "[]", "/games/starcraft-ii.zip", "Легендарна космічна стратегія реального часу.", 42949672960L, "[\"Real-Time\",\"Multiplayer\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "StarCraft II", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111127"), 4.5, null, new DateTime(2026, 3, 12, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9052), "Тактична покрокова стратегія проти інопланетних загарбників.", 15, 187075L, "[0]", "[2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 18000000000L, 1125, "[]", "/games/xcom-2.zip", "Тактична покрокова стратегія проти інопланетних загарбників.", 61203283968L, "[\"Turn-Based\",\"Sci-Fi\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "XCOM 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111128"), 4.7000000000000002, null, new DateTime(2026, 2, 2, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9121), "Симулятор будівництва та управління містом.", 25, 11413L, "[0]", "[6,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 14000000000L, 463, "[]", "/games/cities-skylines.zip", "Симулятор будівництва та управління містом.", 37580963840L, "[\"Open World\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Cities: Skylines", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111129"), 4.7000000000000002, null, new DateTime(2026, 5, 3, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9163), "Симулятор життя з безліччю можливостей кастомізації.", 25, 28423L, "[0]", "[6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 10000000000L, 2473, "[]", "/games/the-sims-4.zip", "Симулятор життя з безліччю можливостей кастомізації.", 5368709120L, "[\"Casual\",\"Singleplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "The Sims 4", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111130"), 3.7999999999999998, null, new DateTime(2025, 12, 19, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9202), "Реалістичний симулятор далекобійника Європою.", 25, 162858L, "[0]", "[6,10]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 8000000000L, 1908, "[]", "/games/euro-truck-simulator-2.zip", "Реалістичний симулятор далекобійника Європою.", 21474836480L, "[\"Open World\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Euro Truck Simulator 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111131"), 3.6000000000000001, null, new DateTime(2026, 6, 19, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9296), "Детальний симулятор сучасного фермера.", 15, 14776L, "[0,1]", "[6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 16000000000L, 3826, "[]", "/games/farming-simulator-22.zip", "Детальний симулятор сучасного фермера.", 19327352832L, "[\"Open World\",\"Casual\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Farming Simulator 22", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111132"), 4.2000000000000002, null, new DateTime(2026, 7, 1, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9340), "Аркадні перегони у відкритому світі Мексики.", 0, 175464L, "[0,1]", "[10]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 25000000000L, 4514, "[]", "/games/forza-horizon-5.zip", "Аркадні перегони у відкритому світі Мексики.", 6442450944L, "[\"Open World\",\"Multiplayer\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Forza Horizon 5", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111133"), 4.0, null, new DateTime(2025, 4, 27, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9411), "Нічні вуличні перегони з поліцейськими погонями.", 50, 137394L, "[0,1]", "[10,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 15000000000L, 1444, "[]", "/games/need-for-speed-heat.zip", "Нічні вуличні перегони з поліцейськими погонями.", 17179869184L, "[\"Open World\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Need for Speed Heat", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111134"), 3.8999999999999999, null, new DateTime(2024, 5, 21, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9450), "Футбол на ракетних автомобілях.", 30, 96035L, "[1]", "[9,10]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 0L, 85, "[]", "/games/rocket-league.zip", "Футбол на ракетних автомобілях.", 61203283968L, "[\"Multiplayer\",\"Real-Time\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Rocket League", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111135"), 4.2999999999999998, null, new DateTime(2024, 5, 17, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9488), "Найпопулярніший футбольний симулятор.", 0, 176139L, "[0,1]", "[9]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 22000000000L, 189, "[]", "/games/fifa-23.zip", "Найпопулярніший футбольний симулятор.", 65498251264L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "FIFA 23", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111136"), 3.6000000000000001, null, new DateTime(2026, 8, 12, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9527), "Реалістичний баскетбольний симулятор.", 15, 48822L, "[0,1]", "[9]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 22000000000L, 2872, "[]", "/games/nba-2k24.zip", "Реалістичний баскетбольний симулятор.", 25769803776L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "NBA 2K24", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111137"), 4.7999999999999998, null, new DateTime(2025, 7, 18, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9592), "Атмосферний survival horror від першої особи.", 30, 94712L, "[0]", "[8,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 3762, "[]", "/games/resident-evil-village.zip", "Атмосферний survival horror від першої особи.", 57982058496L, "[\"Atmospheric\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Resident Evil Village", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111138"), 3.3999999999999999, null, new DateTime(2025, 1, 15, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9637), "Хоррор від першої особи в покинутій психлікарні.", 0, 1196L, "[0]", "[8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 6000000000L, 246, "[]", "/games/outlast.zip", "Хоррор від першої особи в покинутій психлікарні.", 62277025792L, "[\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Outlast", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111139"), 4.5, null, new DateTime(2026, 8, 3, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9733), "Кооперативний хоррор про полювання на привидів.", 15, 187831L, "[2]", "[8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 7000000000L, 1881, "[]", "/games/phasmophobia.zip", "Кооперативний хоррор про полювання на привидів.", 35433480192L, "[\"Multiplayer\",\"Co-op\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Phasmophobia", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111140"), 4.0999999999999996, null, new DateTime(2025, 9, 17, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9776), "Асиметричний мультиплеєрний хоррор.", 0, 167951L, "[1]", "[8,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 10000000000L, 2001, "[]", "/games/dead-by-daylight.zip", "Асиметричний мультиплеєрний хоррор.", 56908316672L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Dead by Daylight", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111141"), 3.5, null, new DateTime(2024, 11, 21, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9849), "Кооперативний шутер проти орд зомбі.", 10, 18051L, "[2]", "[4,8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 5000000000L, 2101, "[]", "/games/left-4-dead-2.zip", "Кооперативний шутер проти орд зомбі.", 56908316672L, "[\"Co-op\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Left 4 Dead 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111142"), 3.7999999999999998, null, new DateTime(2025, 10, 8, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9892), "Величезна CRPG на основі D&D з глибоким сюжетом.", 25, 109030L, "[0,2]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 32000000000L, 3080, "[]", "/games/baldurs-gate-3.zip", "Величезна CRPG на основі D&D з глибоким сюжетом.", 34359738368L, "[\"Story Rich\",\"Fantasy\",\"Turn-Based\",\"Co-op\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Baldur's Gate 3", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111143"), 4.5, null, new DateTime(2025, 11, 6, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9930), "Покрокова RPG з кооперативом та величезною свободою дій.", 15, 62401L, "[0,2]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 20000000000L, 1451, "[]", "/games/divinity-original-sin-2.zip", "Покрокова RPG з кооперативом та величезною свободою дій.", 3221225472L, "[\"Turn-Based\",\"Fantasy\",\"Story Rich\",\"Co-op\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Divinity: Original Sin 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111144"), 3.2000000000000002, null, new DateTime(2026, 8, 16, 5, 47, 50, 828, DateTimeKind.Utc).AddTicks(9969), "Детективна RPG з унікальним текстовим наративом.", 0, 115618L, "[0]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 18000000000L, 4668, "[]", "/games/disco-elysium.zip", "Детективна RPG з унікальним текстовим наративом.", 21474836480L, "[\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Disco Elysium", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111145"), 4.4000000000000004, null, new DateTime(2025, 5, 29, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(12), "Космічна RPG-трилогія про капітана Шепарда.", 10, 178462L, "[0]", "[1,4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 24000000000L, 2512, "[]", "/games/mass-effect-legendary-edition.zip", "Космічна RPG-трилогія про капітана Шепарда.", 47244640256L, "[\"Sci-Fi\",\"Story Rich\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Mass Effect Legendary Edition", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111146"), 3.2000000000000002, null, new DateTime(2025, 9, 8, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(136), "Стильна JRPG про підлітків-фантомних злодіїв.", 0, 90960L, "[0]", "[1,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 25000000000L, 5010, "[]", "/games/persona-5-royal.zip", "Стильна JRPG про підлітків-фантомних злодіїв.", 2147483648L, "[\"Anime\",\"Story Rich\",\"Turn-Based\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Persona 5 Royal", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111147"), 4.5, null, new DateTime(2024, 11, 29, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(177), "Ремейк культової JRPG про боротьбу з корпорацією Shinra.", 15, 79543L, "[0]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 28000000000L, 3593, "[]", "/games/final-fantasy-vii-remake.zip", "Ремейк культової JRPG про боротьбу з корпорацією Shinra.", 48318382080L, "[\"Anime\",\"Story Rich\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Final Fantasy VII Remake", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111148"), 3.7999999999999998, null, new DateTime(2025, 1, 29, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(215), "Полювання на гігантських монстрів у команді.", 25, 14382L, "[0,2]", "[0,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 3432, "[]", "/games/monster-hunter-world.zip", "Полювання на гігантських монстрів у команді.", 47244640256L, "[\"Co-op\",\"Multiplayer\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Monster Hunter: World", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111149"), 3.5, null, new DateTime(2025, 6, 7, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(258), "Безкоштовний кооперативний looter-shooter у космосі.", 10, 3853L, "[2]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 2903, "[]", "/games/warframe.zip", "Безкоштовний кооперативний looter-shooter у космосі.", 37580963840L, "[\"Free to Play\",\"Co-op\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Warframe", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111150"), 4.7999999999999998, null, new DateTime(2026, 8, 18, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(323), "Науково-фантастичний looter-shooter з великим світом.", 30, 103916L, "[1,2]", "[4,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 2966, "[]", "/games/destiny-2.zip", "Науково-фантастичний looter-shooter з великим світом.", 19327352832L, "[\"Free to Play\",\"Multiplayer\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Destiny 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111151"), 3.7999999999999998, null, new DateTime(2025, 8, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(366), "Динамічний battle royale з героями.", 25, 99184L, "[1]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 3234, "[]", "/games/apex-legends.zip", "Динамічний battle royale з героями.", 27917287424L, "[\"Free to Play\",\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Apex Legends", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111152"), 3.7000000000000002, null, new DateTime(2025, 10, 27, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(404), "Найпопулярніший battle royale з будівництвом.", 20, 166911L, "[1]", "[4,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 0L, 961, "[]", "/games/fortnite.zip", "Найпопулярніший battle royale з будівництвом.", 13958643712L, "[\"Free to Play\",\"Multiplayer\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Fortnite", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111153"), 4.2999999999999998, null, new DateTime(2024, 10, 8, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(499), "Тактичний командний шутер з унікальними агентами.", 0, 13995L, "[1]", "[4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 3045, "[]", "/games/valorant.zip", "Тактичний командний шутер з унікальними агентами.", 39728447488L, "[\"Free to Play\",\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Valorant", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111154"), 3.8999999999999999, null, new DateTime(2026, 2, 10, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(585), "Командний шутер-герой з яскравими персонажами.", 30, 61205L, "[1]", "[4,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 255, "[]", "/games/overwatch-2.zip", "Командний шутер-герой з яскравими персонажами.", 28991029248L, "[\"Free to Play\",\"Multiplayer\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Overwatch 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111155"), 4.9000000000000004, null, new DateTime(2026, 6, 24, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(631), "Класичний командний шутер з гумором.", 50, 166971L, "[1]", "[4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 0L, 1021, "[]", "/games/team-fortress-2.zip", "Класичний командний шутер з гумором.", 13958643712L, "[\"Free to Play\",\"Multiplayer\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Team Fortress 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111156"), 4.7000000000000002, null, new DateTime(2025, 3, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(683), "Тактичний шутер з руйнівним оточенням.", 25, 58637L, "[1]", "[4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 15000000000L, 2687, "[]", "/games/rainbow-six-siege.zip", "Тактичний шутер з руйнівним оточенням.", 63350767616L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Rainbow Six Siege", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111157"), 4.0, null, new DateTime(2024, 10, 11, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(734), "Один з перших масових battle royale.", 50, 158892L, "[1]", "[4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 2942, "[]", "/games/pubg-battlegrounds.zip", "Один з перших масових battle royale.", 36507222016L, "[\"Free to Play\",\"Multiplayer\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "PUBG: Battlegrounds", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111158"), 4.5999999999999996, null, new DateTime(2026, 2, 21, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(780), "Класичний файтинг нового покоління.", 20, 134494L, "[1]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 3544, "[]", "/games/street-fighter-6.zip", "Класичний файтинг нового покоління.", 17179869184L, "[\"Multiplayer\",\"Real-Time\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Street Fighter 6", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111159"), 4.0, null, new DateTime(2024, 8, 18, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(853), "Динамічний 3D-файтинг з великим ростером бійців.", 50, 107546L, "[1]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 22000000000L, 1596, "[]", "/games/tekken-8.zip", "Динамічний 3D-файтинг з великим ростером бійців.", 30064771072L, "[\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Tekken 8", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111160"), 3.7999999999999998, null, new DateTime(2024, 10, 13, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(950), "Жорстокий файтинг з фаталіті.", 25, 67990L, "[0,1]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 24000000000L, 2040, "[]", "/games/mortal-kombat-1.zip", "Жорстокий файтинг з фаталіті.", 34359738368L, "[\"Multiplayer\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Mortal Kombat 1", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111161"), 3.2000000000000002, null, new DateTime(2026, 7, 29, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(991), "Кросовер-файтинг з персонажами Nintendo.", 0, 136936L, "[1]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 986, "[]", "/games/super-smash-bros-ultimate.zip", "Кросовер-файтинг з персонажами Nintendo.", 40802189312L, "[\"Multiplayer\",\"Funny\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Super Smash Bros. Ultimate", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111162"), 4.2000000000000002, null, new DateTime(2025, 1, 25, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1030), "Складний ран-н-ган у стилі мультфільмів 1930-х.", 0, 165286L, "[0,2]", "[0,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 8000000000L, 4336, "[]", "/games/cuphead.zip", "Складний ран-н-ган у стилі мультфільмів 1930-х.", 51539607552L, "[\"Difficult\",\"Atmospheric\",\"Co-op\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Cuphead", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111163"), 4.0, null, new DateTime(2026, 4, 22, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1100), "Прекрасний атмосферний платформер-метроідванія.", 50, 144834L, "[0]", "[3,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 7000000000L, 3884, "[]", "/games/ori-and-the-blind-forest.zip", "Прекрасний атмосферний платформер-метроідванія.", 17179869184L, "[\"Atmospheric\",\"Story Rich\",\"Great Soundtrack\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Ori and the Blind Forest", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111164"), 3.6000000000000001, null, new DateTime(2025, 8, 17, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1143), "Унікальна RPG про вибір між боєм та миром.", 15, 35782L, "[0]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 4000000000L, 4832, "[]", "/games/undertale.zip", "Унікальна RPG про вибір між боєм та миром.", 25769803776L, "[\"Story Rich\",\"Pixel Art\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Undertale", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111165"), 4.0, null, new DateTime(2026, 6, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1186), "Атмосферний піксельний action-adventure.", 50, 42080L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 6000000000L, 1130, "[]", "/games/hyper-light-drifter.zip", "Атмосферний піксельний action-adventure.", 23622320128L, "[\"Pixel Art\",\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Hyper Light Drifter", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111166"), 4.2000000000000002, null, new DateTime(2024, 5, 18, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1225), "Стильний неонуарний платформер про вбивцю з катаною.", 0, 139738L, "[0]", "[0,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 6000000000L, 3788, "[]", "/games/katana-zero.zip", "Стильний неонуарний платформер про вбивцю з катаною.", 64424509440L, "[\"Pixel Art\",\"Story Rich\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Katana ZERO", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111167"), 4.2999999999999998, null, new DateTime(2025, 10, 3, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1321), "Детективна головоломка про розслідування зникнення екіпажу.", 0, 48035L, "[0]", "[5,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 9000000000L, 2085, "[]", "/games/return-of-the-obra-dinn.zip", "Детективна головоломка про розслідування зникнення екіпажу.", 39728447488L, "[\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Return of the Obra Dinn", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111168"), 3.3999999999999999, null, new DateTime(2026, 6, 21, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1366), "Сатирична гра про вибір і наратив.", 0, 114374L, "[0]", "[3,5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 5000000000L, 3424, "[]", "/games/the-stanley-parable.zip", "Сатирична гра про вибір і наратив.", 17179869184L, "[\"Story Rich\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "The Stanley Parable", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111169"), 4.2999999999999998, null, new DateTime(2026, 3, 14, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1406), "Атмосферна пригода про самотнього лісника.", 0, 151073L, "[0]", "[3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 7000000000L, 123, "[]", "/games/firewatch.zip", "Атмосферна пригода про самотнього лісника.", 59055800320L, "[\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Firewatch", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111170"), 3.7999999999999998, null, new DateTime(2025, 9, 2, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1443), "Емоційна історична пригода про родину Фінч.", 25, 15266L, "[0]", "[3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 6000000000L, 4316, "[]", "/games/what-remains-of-edith-finch.zip", "Емоційна історична пригода про родину Фінч.", 8589934592L, "[\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "What Remains of Edith Finch", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111171"), 4.5999999999999996, null, new DateTime(2025, 9, 12, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1483), "Похмура кінематографічна головоломка-платформер.", 20, 162756L, "[0]", "[5,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 5000000000L, 1806, "[]", "/games/inside.zip", "Похмура кінематографічна головоломка-платформер.", 62277025792L, "[\"Atmospheric\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Inside", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111172"), 3.2999999999999998, null, new DateTime(2024, 11, 23, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1548), "Чорно-біла атмосферна головоломка-платформер.", 0, 24549L, "[0]", "[5,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 4000000000L, 3599, "[]", "/games/limbo.zip", "Чорно-біла атмосферна головоломка-платформер.", 54760833024L, "[\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Limbo", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111173"), 3.8999999999999999, null, new DateTime(2026, 4, 5, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1593), "Кумедна кооперативна фізична головоломка.", 30, 157651L, "[2]", "[5,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 6000000000L, 1701, "[]", "/games/human-fall-flat.zip", "Кумедна кооперативна фізична головоломка.", 35433480192L, "[\"Co-op\",\"Funny\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Human: Fall Flat", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111174"), 4.5, null, new DateTime(2025, 2, 9, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1725), "Кооперативна пригода про пару, що стала ляльками.", 15, 71371L, "[2]", "[3,5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 14000000000L, 421, "[]", "/games/it-takes-two.zip", "Кооперативна пригода про пару, що стала ляльками.", 35433480192L, "[\"Co-op\",\"Story Rich\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "It Takes Two", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111175"), 4.5999999999999996, null, new DateTime(2026, 7, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1769), "Кооперативний екшн про втечу з в'язниці.", 20, 149350L, "[2]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 12000000000L, 3400, "[]", "/games/a-way-out.zip", "Кооперативний екшн про втечу з в'язниці.", 55834574848L, "[\"Co-op\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "A Way Out", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111176"), 4.4000000000000004, null, new DateTime(2026, 2, 23, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1837), "Хаотична кооперативна кулінарна аркада.", 10, 132192L, "[2]", "[6,5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 8000000000L, 1242, "[]", "/games/overcooked-2.zip", "Хаотична кооперативна кулінарна аркада.", 15032385536L, "[\"Co-op\",\"Funny\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Overcooked! 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111177"), 4.7999999999999998, null, new DateTime(2024, 3, 19, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1878), "Соціальна гра на виявлення зрадника серед екіпажу.", 30, 17898L, "[1]", "[5,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 3000000000L, 1948, "[]", "/games/among-us.zip", "Соціальна гра на виявлення зрадника серед екіпажу.", 64424509440L, "[\"Multiplayer\",\"Funny\",\"Casual\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Among Us", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111178"), 4.0999999999999996, null, new DateTime(2024, 11, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1917), "Кумедний battle royale-платформер з перешкодами.", 0, 83957L, "[1]", "[0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 3007, "[]", "/games/fall-guys.zip", "Кумедний battle royale-платформер з перешкодами.", 63350767616L, "[\"Free to Play\",\"Multiplayer\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Fall Guys", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111179"), 3.3999999999999999, null, new DateTime(2024, 7, 19, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1957), "Хардкорний тактичний шутер-виживання.", 0, 142476L, "[1]", "[4,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 1526, "[]", "/games/escape-from-tarkov.zip", "Хардкорний тактичний шутер-виживання.", 62277025792L, "[\"Difficult\",\"Multiplayer\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Escape from Tarkov", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111180"), 4.2000000000000002, null, new DateTime(2025, 11, 27, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(1995), "Жорсткий мультиплеєрний симулятор виживання.", 0, 159980L, "[1]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 12000000000L, 4030, "[]", "/games/rust.zip", "Жорсткий мультиплеєрний симулятор виживання.", 45097156608L, "[\"Multiplayer\",\"Open World\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Rust", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111181"), 4.7999999999999998, null, new DateTime(2024, 11, 26, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2164), "Постапокаліптичний симулятор виживання серед зомбі.", 30, 123846L, "[1]", "[6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 14000000000L, 2896, "[]", "/games/dayz.zip", "Постапокаліптичний симулятор виживання серед зомбі.", 51539607552L, "[\"Open World\",\"Post-Apocalyptic\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "DayZ", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111182"), 4.0, null, new DateTime(2026, 1, 4, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2208), "Кооперативний вікінгівський survival-сендбокс.", 50, 72842L, "[2]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 10000000000L, 1892, "[]", "/games/valheim.zip", "Кооперативний вікінгівський survival-сендбокс.", 4294967296L, "[\"Co-op\",\"Open World\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Valheim", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111183"), 4.2000000000000002, null, new DateTime(2026, 3, 15, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2248), "Хоррор-виживання на острові з канібалами.", 0, 12372L, "[0,2]", "[6,8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 9000000000L, 1422, "[]", "/games/the-forest.zip", "Хоррор-виживання на острові з канібалами.", 57982058496L, "[\"Open World\",\"Co-op\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "The Forest", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111184"), 4.0999999999999996, null, new DateTime(2026, 1, 21, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2288), "Виживання в підводному інопланетному світі.", 0, 192125L, "[0]", "[6,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 12000000000L, 1175, "[]", "/games/subnautica.zip", "Виживання в підводному інопланетному світі.", 50465865728L, "[\"Open World\",\"Sci-Fi\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Subnautica", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111185"), 3.3999999999999999, null, new DateTime(2025, 8, 19, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2354), "Космічна пісочниця з процедурним всесвітом.", 0, 88880L, "[0,1]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 2930, "[]", "/games/no-mans-sky.zip", "Космічна пісочниця з процедурним всесвітом.", 23622320128L, "[\"Open World\",\"Sci-Fi\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "No Man's Sky", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111186"), 3.2999999999999998, null, new DateTime(2025, 5, 22, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2397), "Виживання серед динозаврів у відкритому світі.", 0, 71469L, "[0,1]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 15000000000L, 519, "[]", "/games/ark-survival-evolved.zip", "Виживання серед динозаврів у відкритому світі.", 54760833024L, "[\"Open World\",\"Multiplayer\",\"Co-op\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "ARK: Survival Evolved", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111187"), 4.4000000000000004, null, new DateTime(2025, 8, 9, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2437), "3D рогалик-шутер з кооперативом.", 10, 199590L, "[0,2]", "[5,4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 8000000000L, 3640, "[]", "/games/risk-of-rain-2.zip", "3D рогалик-шутер з кооперативом.", 34359738368L, "[\"Co-op\",\"Sci-Fi\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Risk of Rain 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111188"), 3.7999999999999998, null, new DateTime(2025, 4, 11, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2477), "Рогалик-шутер з видом зверху та купою куль.", 25, 89710L, "[0,2]", "[5,4]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 7000000000L, 3760, "[]", "/games/enter-the-gungeon.zip", "Рогалик-шутер з видом зверху та купою куль.", 34359738368L, "[\"Pixel Art\",\"Co-op\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Enter the Gungeon", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111189"), 4.2999999999999998, null, new DateTime(2024, 7, 28, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2550), "Аддиктивний рогалик про виживання проти орд ворогів.", 0, 120467L, "[0]", "[5,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 3000000000L, 4517, "[]", "/games/vampire-survivors.zip", "Аддиктивний рогалик про виживання проти орд ворогів.", 52613349376L, "[\"Pixel Art\",\"Casual\",\"Real-Time\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Vampire Survivors", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111190"), 4.0, null, new DateTime(2024, 7, 13, 5, 47, 50, 829, DateTimeKind.Utc).AddTicks(2591), "Покроковий рогалик-менеджмент космічного корабля.", 50, 5682L, "[0]", "[5,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 5000000000L, 4732, "[]", "/games/ftl-faster-than-light.zip", "Покроковий рогалик-менеджмент космічного корабля.", 4294967296L, "[\"Sci-Fi\",\"Turn-Based\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "FTL: Faster Than Light", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111191"), 4.2000000000000002, null, new DateTime(2026, 1, 20, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(6951), "Похмурий покроковий рогалик з менеджментом стресу героїв.", 0, 140626L, "[0]", "[5,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 9000000000L, 4676, "[]", "/games/darkest-dungeon.zip", "Похмурий покроковий рогалик з менеджментом стресу героїв.", 51539607552L, "[\"Turn-Based\",\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Darkest Dungeon", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111192"), 4.7999999999999998, null, new DateTime(2025, 8, 23, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7050), "Тактичний покроковий рогалик про захист від монстрів.", 30, 152776L, "[0]", "[5,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 6000000000L, 1826, "[]", "/games/into-the-breach.zip", "Тактичний покроковий рогалик про захист від монстрів.", 19327352832L, "[\"Turn-Based\",\"Sci-Fi\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Into the Breach", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111193"), 4.7000000000000002, null, new DateTime(2024, 10, 4, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7173), "Продовження культового рогалика про підземне царство.", 25, 170799L, "[0]", "[5,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 15000000000L, 4849, "[]", "/games/hades-ii.zip", "Продовження культового рогалика про підземне царство.", 44023414784L, "[\"Fantasy\",\"Story Rich\",\"Great Soundtrack\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Hades II", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111194"), 4.4000000000000004, null, new DateTime(2026, 6, 29, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7221), "MMORPG з динамічною бойовою системою.", 10, 22866L, "[1]", "[7,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 0L, 1916, "[]", "/games/guild-wars-2.zip", "MMORPG з динамічною бойовою системою.", 8589934592L, "[\"Fantasy\",\"Open World\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Guild Wars 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111195"), 3.7000000000000002, null, new DateTime(2025, 7, 29, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7280), "Популярна MMORPG з глибоким сюжетом.", 20, 13001L, "[1]", "[7,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 20000000000L, 2051, "[]", "/games/final-fantasy-xiv.zip", "Популярна MMORPG з глибоким сюжетом.", 46170898432L, "[\"Fantasy\",\"Story Rich\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Final Fantasy XIV", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111196"), 4.7000000000000002, null, new DateTime(2026, 5, 3, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7320), "Найвідоміша MMORPG усіх часів.", 25, 134523L, "[1]", "[7,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 20000000000L, 3573, "[]", "/games/world-of-warcraft.zip", "Найвідоміша MMORPG усіх часів.", 5368709120L, "[\"Fantasy\",\"Multiplayer\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "World of Warcraft", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111197"), 3.7000000000000002, null, new DateTime(2025, 11, 14, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7391), "Безкоштовна дарк-фентезійна ARPG з глибокою кастомізацією.", 20, 38793L, "[0,1]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 0L, 2843, "[]", "/games/path-of-exile.zip", "Безкоштовна дарк-фентезійна ARPG з глибокою кастомізацією.", 59055800320L, "[\"Free to Play\",\"Fantasy\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Path of Exile", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111198"), 3.3999999999999999, null, new DateTime(2024, 9, 29, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7438), "Похмура екшн-RPG про боротьбу з демонами.", 0, 5004L, "[0,1]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 30000000000L, 4054, "[]", "/games/diablo-iv.zip", "Похмура екшн-RPG про боротьбу з демонами.", 49392123904L, "[\"Fantasy\",\"Open World\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Diablo IV", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111199"), 4.4000000000000004, null, new DateTime(2024, 12, 18, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7487), "Класична фентезійна RPG з моральним вибором.", 10, 34824L, "[0]", "[1,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 12000000000L, 3874, "[]", "/games/fable.zip", "Класична фентезійна RPG з моральним вибором.", 27917287424L, "[\"Fantasy\",\"Story Rich\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Fable", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111200"), 4.2999999999999998, null, new DateTime(2024, 12, 1, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7528), "Реалістична середньовічна RPG.", 0, 42941L, "[0]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 1991, "[]", "/games/kingdom-come-deliverance.zip", "Реалістична середньовічна RPG.", 46170898432L, "[\"Open World\",\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Kingdom Come: Deliverance", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111201"), 3.7000000000000002, null, new DateTime(2026, 5, 13, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7567), "Відкритий світ вікінгівської Англії.", 20, 148713L, "[0]", "[0,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 28000000000L, 2763, "[]", "/games/assassins-creed-valhalla.zip", "Відкритий світ вікінгівської Англії.", 59055800320L, "[\"Open World\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Assassin's Creed Valhalla", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111202"), 3.6000000000000001, null, new DateTime(2025, 9, 22, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7635), "Кінематографічна екшн-пригода про самурая.", 15, 66546L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 30000000000L, 596, "[]", "/games/ghost-of-tsushima.zip", "Кінематографічна екшн-пригода про самурая.", 51539607552L, "[\"Open World\",\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Ghost of Tsushima", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111203"), 4.0999999999999996, null, new DateTime(2026, 8, 7, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7682), "Епічна екшн-пригода про Кратоса та скандинавську міфологію.", 0, 141227L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 25000000000L, 277, "[]", "/games/god-of-war.zip", "Епічна екшн-пригода про Кратоса та скандинавську міфологію.", 31138512896L, "[\"Story Rich\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "God of War", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111204"), 3.5, null, new DateTime(2025, 10, 11, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7722), "Постапокаліптична пригода з механічними тваринами.", 10, 54927L, "[0]", "[0,1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 22000000000L, 3977, "[]", "/games/horizon-zero-dawn.zip", "Постапокаліптична пригода з механічними тваринами.", 31138512896L, "[\"Open World\",\"Post-Apocalyptic\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Horizon Zero Dawn", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111205"), 4.5999999999999996, null, new DateTime(2025, 9, 12, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7763), "Незвичайна пригода про доставку вантажів у постапокаліпсисі.", 20, 29656L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 3706, "[]", "/games/death-stranding.zip", "Незвичайна пригода про доставку вантажів у постапокаліпсисі.", 62277025792L, "[\"Open World\",\"Post-Apocalyptic\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Death Stranding", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111206"), 3.2999999999999998, null, new DateTime(2024, 4, 21, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7837), "Містичний екшн у секретній урядовій будівлі.", 0, 113765L, "[0]", "[0,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 2815, "[]", "/games/control.zip", "Містичний екшн у секретній урядовій будівлі.", 28991029248L, "[\"Sci-Fi\",\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Control", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111207"), 4.7999999999999998, null, new DateTime(2026, 7, 31, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7881), "Психологічний хоррор-трилер про письменника.", 30, 76134L, "[0]", "[8,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 28000000000L, 184, "[]", "/games/alan-wake-2.zip", "Психологічний хоррор-трилер про письменника.", 38654705664L, "[\"Story Rich\",\"Atmospheric\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Alan Wake 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111208"), 3.7999999999999998, null, new DateTime(2025, 9, 2, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7922), "Класичний психологічний хоррор.", 25, 187366L, "[0]", "[8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 1416, "[]", "/games/silent-hill-2.zip", "Класичний психологічний хоррор.", 8589934592L, "[\"Atmospheric\",\"Story Rich\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Silent Hill 2", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111209"), 3.5, null, new DateTime(2026, 4, 27, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(7964), "Класичний хоррор без можливості захищатись.", 10, 87029L, "[0]", "[8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 8000000000L, 1079, "[]", "/games/amnesia-the-dark-descent.zip", "Класичний хоррор без можливості захищатись.", 11811160064L, "[\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Amnesia: The Dark Descent", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111210"), 3.7999999999999998, null, new DateTime(2025, 10, 8, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8003), "Хоррор про виживання в піцерії з аніматрониками.", 25, 37930L, "[0]", "[8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 3000000000L, 1980, "[]", "/games/five-nights-at-freddys.zip", "Хоррор про виживання в піцерії з аніматрониками.", 34359738368L, "[\"Atmospheric\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Five Nights at Freddy's", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111211"), 3.7999999999999998, null, new DateTime(2024, 8, 2, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8079), "Атмосферний хоррор-платформер.", 25, 95562L, "[0]", "[8,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 10000000000L, 4612, "[]", "/games/little-nightmares-ii.zip", "Атмосферний хоррор-платформер.", 47244640256L, "[\"Atmospheric\",\"Story Rich\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Little Nightmares II", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111212"), 4.5999999999999996, null, new DateTime(2025, 6, 14, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8131), "Готичний екшн-RPG від FromSoftware.", 20, 177546L, "[0]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 1596, "[]", "/games/bloodborne.zip", "Готичний екшн-RPG від FromSoftware.", 30064771072L, "[\"Difficult\",\"Atmospheric\",\"Fantasy\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Bloodborne", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111213"), 3.7999999999999998, null, new DateTime(2024, 9, 7, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8180), "Філософська екшн-RPG про андроїдів.", 25, 74726L, "[0]", "[1,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 20000000000L, 3776, "[]", "/games/nier-automata.zip", "Філософська екшн-RPG про андроїдів.", 8589934592L, "[\"Story Rich\",\"Sci-Fi\",\"Great Soundtrack\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Nier: Automata", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111214"), 3.6000000000000001, null, new DateTime(2025, 12, 21, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8219), "Відкритий світ у стилі аніме з гача-механікою.", 15, 103556L, "[0,1]", "[1,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 2606, "[]", "/games/genshin-impact.zip", "Відкритий світ у стилі аніме з гача-механікою.", 19327352832L, "[\"Free to Play\",\"Anime\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Genshin Impact", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111215"), 3.6000000000000001, null, new DateTime(2026, 7, 7, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8286), "Покрокова аніме-RPG у космічному сеттингу.", 15, 140758L, "[0]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 0L, 4808, "[]", "/games/honkai-star-rail.zip", "Покрокова аніме-RPG у космічному сеттингу.", 64424509440L, "[\"Free to Play\",\"Anime\",\"Turn-Based\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Honkai: Star Rail", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111216"), 4.7999999999999998, null, new DateTime(2026, 8, 18, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8328), "Візуальна новела-детектив зі смертельною грою.", 30, 86316L, "[0]", "[5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 15000000000L, 366, "[]", "/games/danganronpa-trigger-happy-havoc.zip", "Візуальна новела-детектив зі смертельною грою.", 19327352832L, "[\"Story Rich\",\"Anime\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Danganronpa: Trigger Happy Havoc", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111217"), 4.9000000000000004, null, new DateTime(2026, 5, 1, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8369), "Психологічна візуальна новела, що ламає жанрові кліше.", 50, 186925L, "[0]", "[5,8]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 0L, 975, "[]", "/games/doki-doki-literature-club.zip", "Психологічна візуальна новела, що ламає жанрові кліше.", 7516192768L, "[\"Story Rich\",\"Anime\",\"Free to Play\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Doki Doki Literature Club", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111218"), 3.8999999999999999, null, new DateTime(2026, 4, 23, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8410), "Культова JRPG про розкриття таємниці в маленькому місті.", 30, 186833L, "[0]", "[1]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 883, "[]", "/games/persona-4-golden.zip", "Культова JRPG про розкриття таємниці в маленькому місті.", 16106127360L, "[\"Anime\",\"Story Rich\",\"Turn-Based\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Persona 4 Golden", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111219"), 4.4000000000000004, null, new DateTime(2025, 11, 25, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8479), "Флагманська VR-пригода у всесвіті Half-Life.", 10, 194382L, "[0]", "[4,3]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 25000000000L, 3432, "[]", "/games/half-life-alyx.zip", "Флагманська VR-пригода у всесвіті Half-Life.", 47244640256L, "[\"VR\",\"Story Rich\",\"Sci-Fi\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Half-Life: Alyx", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111220"), 4.7000000000000002, null, new DateTime(2026, 2, 20, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8531), "Ритм-гра з мечами у VR.", 25, 199295L, "[0]", "[5]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 15000000000L, 3345, "[]", "/games/beat-saber.zip", "Ритм-гра з мечами у VR.", 18253611008L, "[\"VR\",\"Multiplayer\",\"Great Soundtrack\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Beat Saber", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111221"), 3.6000000000000001, null, new DateTime(2025, 9, 4, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8573), "Фізичний VR-симулятор середньовічних боїв.", 15, 164664L, "[0]", "[6,0]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 6000000000L, 3714, "[]", "/games/half-sword.zip", "Фізичний VR-симулятор середньовічних боїв.", 6442450944L, "[\"VR\",\"Early Access\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Half Sword", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111222"), 3.2000000000000002, null, new DateTime(2026, 4, 30, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8622), "Індустріальний симулятор побудови фабрик у 3D.", 0, 148226L, "[0,2]", "[6,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 18000000000L, 2276, "[]", "/games/satisfactory.zip", "Індустріальний симулятор побудови фабрик у 3D.", 8589934592L, "[\"Open World\",\"Co-op\",\"Early Access\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Satisfactory", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111223"), 4.0999999999999996, null, new DateTime(2026, 8, 25, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8672), "Симулятор автоматизації виробництва на іншій планеті.", 0, 18109L, "[0,1]", "[6,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1,2]", 20000000000L, 2159, "[]", "/games/factorio.zip", "Симулятор автоматизації виробництва на іншій планеті.", 11811160064L, "[\"Sci-Fi\",\"Difficult\",\"Multiplayer\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Factorio", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111224"), 4.5999999999999996, null, new DateTime(2024, 10, 5, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8723), "Гумористичний симулятор управління лікарнею.", 20, 199398L, "[0]", "[6,2]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 12000000000L, 3448, "[]", "/games/two-point-hospital.zip", "Гумористичний симулятор управління лікарнею.", 42949672960L, "[\"Casual\",\"Funny\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Two Point Hospital", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111225"), 3.2999999999999998, null, new DateTime(2025, 12, 6, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8776), "Детальний симулятор побудови парку атракціонів.", 0, 1571L, "[0]", "[6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0]", 16000000000L, 621, "[]", "/games/planet-coaster.zip", "Детальний симулятор побудови парку атракціонів.", 35433480192L, "[\"Casual\",\"Open World\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Planet Coaster", null, null, "1.0.0" },
                    { new Guid("11111111-1111-1111-1111-111111111226"), 3.7999999999999998, null, new DateTime(2024, 5, 22, 5, 47, 50, 834, DateTimeKind.Utc).AddTicks(8829), "Найдетальніший симулятор футбольного менеджера.", 25, 48334L, "[0]", "[9,6]", null, false, true, new Guid("00000000-0000-0000-0000-000000000001"), null, "[0,1]", 22000000000L, 2384, "[]", "/games/football-manager-2024.zip", "Найдетальніший симулятор футбольного менеджера.", 60129542144L, "[\"Turn-Based\",\"Difficult\"]", "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]", "Football Manager 2024", null, null, "1.0.0" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_ReceiverId",
                table: "FriendRequests",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_FriendRequests_SenderId_ReceiverId",
                table: "FriendRequests",
                columns: new[] { "SenderId", "ReceiverId" });

            migrationBuilder.CreateIndex(
                name: "IX_Games_OwnerId",
                table: "Games",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_ParentGameId",
                table: "Games",
                column: "ParentGameId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_GameId",
                table: "Reviews",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_UserId_GameId",
                table: "Reviews",
                columns: new[] { "UserId", "GameId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tranxactions_TxhHash",
                table: "Tranxactions",
                column: "TxhHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tranxactions_UserId",
                table: "Tranxactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCartItems_GameId",
                table: "UserCartItems",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFriends_FriendId",
                table: "UserFriends",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_UserGames_GameId",
                table: "UserGames",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FamilyOwnerId",
                table: "Users",
                column: "FamilyOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_WalletAddress",
                table: "Users",
                column: "WalletAddress");

            migrationBuilder.CreateIndex(
                name: "IX_UserWishlists_GameId",
                table: "UserWishlists",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FriendRequests");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Tranxactions");

            migrationBuilder.DropTable(
                name: "UserCartItems");

            migrationBuilder.DropTable(
                name: "UserFriends");

            migrationBuilder.DropTable(
                name: "UserGames");

            migrationBuilder.DropTable(
                name: "UserWishlists");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
