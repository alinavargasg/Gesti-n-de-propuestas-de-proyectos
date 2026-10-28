using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthenticationModule.Models.Projects
{
    [Table("docentes")]
    public class Docente
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(2, ErrorMessage = "Debe tener al menos 2 caracteres")]
        [MaxLength(100, ErrorMessage = "No puede exceder 100 caracteres")]
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

        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [MinLength(5, ErrorMessage = "Debe tener al menos 5 caracteres")]
        [MaxLength(200, ErrorMessage = "No puede exceder 200 caracteres")]
        [Column("especialidad")]
        public string Especialidad { get; set; }

        // Relación N:M con Proyecto a través de Evaluacion
        public virtual ICollection<Evaluacion> Evaluaciones { get; set; } = new List<Evaluacion>();
    }
}
