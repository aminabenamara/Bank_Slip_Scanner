using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bank_Slip_Scanner_App.Migrations
{
    /// <inheritdoc />
    public partial class PremierLancement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    idUsers = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TentativesConnexionEchouees = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    mot_de_passe_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    salt = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    nom_complet = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    telephone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    photo_url = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    email_verifie = table.Column<bool>(type: "bit", nullable: false),
                    date_verification_email = table.Column<DateTime>(type: "datetime2", nullable: true),
                    derniere_connexion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tentatives_connexion_echouees = table.Column<int>(type: "int", nullable: false),
                    compte_verrouille = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    date_verrouille = table.Column<DateTime>(type: "datetime2", nullable: true),
                    token_reinitialisation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    date_expiration_token = table.Column<DateTime>(type: "datetime2", nullable: true),
                    date_creation = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    date_modification = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.idUsers);
                });

            migrationBuilder.CreateTable(
                name: "sessions",
                columns: table => new
                {
                    id_session = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idUsers = table.Column<int>(type: "int", nullable: false),
                    token_session = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    refreshe_token = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ip_adress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    user_agent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date_creation = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    date_derniere_activite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    actif = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sessions", x => x.id_session);
                    table.ForeignKey(
                        name: "FK_sessions_Users_idUsers",
                        column: x => x.idUsers,
                        principalTable: "Users",
                        principalColumn: "idUsers",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sessions_idUsers",
                table: "sessions",
                column: "idUsers");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_refreshe_token",
                table: "sessions",
                column: "refreshe_token");

            migrationBuilder.CreateIndex(
                name: "IX_sessions_token_session",
                table: "sessions",
                column: "token_session");

            migrationBuilder.CreateIndex(
                name: "IX_Users_email",
                table: "Users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "sessions");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
