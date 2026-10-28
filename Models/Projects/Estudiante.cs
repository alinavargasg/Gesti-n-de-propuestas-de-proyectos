using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    [Table("estudiantes")]
    public class Estudiante
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Column("nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido paterno es obligatorio")]
        [MinLength(2, ErrorMessage = "Debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Column("apellido_paterno")]
        public string ApellidoPaterno { get; set; }

        [Required(ErrorMessage = "El apellido materno es obligatorio")]
        [MinLength(2, ErrorMessage = "Debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
        [Column("apellido_materno")]
        public string ApellidoMaterno { get; set; }

        [Required(ErrorMessage = "El curso es obligatorio")]
        [MinLength(1, ErrorMessage = "Debe tener al menos 1 carácter")]
        [MaxLength(50, ErrorMessage = "No puede exceder 50 caracteres")]
        [Column("curso")]
        public string Curso { get; set; }

        // FK a Proyecto (nullable: un estudiante puede no tener proyecto)
        [Column("proyecto_id")]
        public int? ProyectoId { get; set; }

        [ForeignKey("ProyectoId")]
        public virtual Proyecto Proyecto { get; set; }
    }
}
