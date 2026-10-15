
namespace Lavillemvc.Models
{
    public class IndexEspecialistas
    {
        // Identificador del especialista
        public int IdEspecialista { get; set; }

        // Especialidad relacionada
        public int IdEspecialidad { get; set; }

        // Nombre completo
        public string Nombre { get; set; }
            = string.Empty;

        // Rol profesional
        public string? RolEspecialista { get; set; }

        // Años de experiencia
        public int? AniosExperiencia { get; set; }

        // Disponibilidad
        public bool? Disponibilidad { get; set; }

        // Horario del especialista
        public string? HorarioEspecialistas { get; set; }

        // Nombre de la especialidad
        // Se obtiene relacionando con ESPECIALIDAD
        public string? NombreEspecialidad { get; set; }
    }
}
