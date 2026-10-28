using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Auth
{
    [Table("roles")]
    public class Role
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        // Almacenamos permisos como JSON para flexibilidad
        [Column("permissions")]
        public string Permissions { get; set; }

        // Relación inversa: Un rol puede tener muchos usuarios
        public virtual ICollection<User> Users { get; set; }
    }
}