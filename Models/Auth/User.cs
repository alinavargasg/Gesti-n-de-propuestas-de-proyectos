using AuthenticationModule.Models.Projects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Auth
{
    [Table("users")] //Nombre de la tabla en PostgreSql
    public class User
    {
        //Primary key: Unique user's identifier
        [Key]
        [Column("id")]
        public int Id { get; set; }

        //Validation Data annotations 
        [Required]
        [MaxLength(100)]
        [Column("username")]
        public string Username { get; set; }

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; }

        //Almacena la contraseña de forma segura: hash + salt
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; }

        
        [Column("password_salt")]
        public string PasswordSalt { get; set; }

        [Column("role_id")]
        public int RoleId { get; set; }

        //Propiedad de navegación que permite acceder al rol del usuario
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        // Campos de auditoría
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("last_login")]
        public DateTime? LastLogin { get; set; }
        // Control de acceso
        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // Seguridad - Prevención de ataques de fuerza bruta
        [Column("failed_login_attempts")]
        public int FailedLoginAttempts { get; set; } = 0;

        [Column("lockout_until")]
        public DateTime? LockoutUntil { get; set; }

        // Para mantener sesión en múltiples dispositivos
        [Column("refresh_token")]
        public string? RefreshToken { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiry { get; set; }

        /// <summary>
        /// ID del docente asociado (si el usuario tiene el rol de Docente).
        /// </summary>
        public int? DocenteId { get; set; }

        [ForeignKey("DocenteId")]
        public virtual Docente Docente { get; set; }

    }
}