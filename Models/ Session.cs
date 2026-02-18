using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Bank_Slip_Scanner_App.Models
{
	[Table("sessions")]
	public class Session
	{
		[Key]
		[Column("id_session")]
		public int IdSession { get; set; }

        [Column("idUsers")]
        public int IdUsers { get; set; }
		[Required]
		[MaxLength(255)]
		[Column("token_session")]
		public string TokenSession { get; set; }
		[MaxLength(255)]
		[Column("refreshe_token")]
		public string RefreshToken { get; set; }
		[MaxLength(45)]
		[Column("ip_adress")]
		public string IpAddress { get; set; }
		[Column("user_agent")]
		public string UserAgent { get; set; }
		[Column("date_creation")]
		public DateTime DateCreation { get; set; } = DateTime.Now;
		[Column("date_expiration")]
		public DateTime DateExpiration {  get; set; }
		[Column("date_derniere_activite")]
		public DateTime DateDerniereActivite { get; set; } = DateTime.Now;
		[Column("actif")]
		public bool Actif {  get; set; } = true;

		[ForeignKey("IdUsers")]
		public virtual Users Users { get; set; }

    }
}