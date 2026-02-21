using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bank_Slip_Scanner_App.Models
{
    [Table("Users")]
    public class Users
    {
        [Key]
        [Column("idUsers")]
        public int IdUsers { get; set; }

        [Required]
        [MaxLength(150)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("mot_de_passe_hash")]
        public string MotDePasseHash { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("salt")]
        public string? Salt { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("nom")]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("prenom")]
        public string Prenom { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("nom_complet")]
        public string? NomComplet { get; set; }

        [MaxLength(20)]
        [Column("telephone")]
        public string? Telephone { get; set; }

        [MaxLength(255)]
        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Column("actif")]
        public bool Actif { get; set; } = true;

        [Column("email_verifie")]
        public bool EmailVerifie { get; set; } = false;

        [Column("date_verification_email")]
        public DateTime? DateVerficationEmail { get; set; }

        [Column("derniere_connexion")]
        public DateTime? DerniereConnexion { get; set; }

        // ON GARDE UNIQUEMENT CELUI-CI QUI CORRESPOND À LA BASE SQL
        [Column("tentatives_connexion_echouees")]
        public int TentativesConnexion { get; set; } = 0;

        [Column("compte_verrouille")]
        public bool CompteVerrouille { get; set; } = false;

        [Column("date_verrouille")]
        public DateTime? DateVerrouillage { get; set; }

        [MaxLength(255)]
        [Column("token_reinitialisation")]
        public string? TokenReinitialisation { get; set; }

        [Column("date_expiration_token")]
        public DateTime? DateExpirationToken { get; set; }

        [Column("date_creation")]
        public DateTime DateCreation { get; set; } = DateTime.Now;

        [Column("date_modification")]
        public DateTime DateModification { get; set; } = DateTime.Now;

        // AJOUT DU MAPPING POUR LE ROLE
        [Column("role")]
        public string Role { get; set; } = "Agent";
    }
}