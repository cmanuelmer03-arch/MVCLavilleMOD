
namespace Lavillemvc.Models
{
    public class IndexServicios
    {
        // Identificador del servicio
        public int IdServicios { get; set; }

        // Especialidad relacionada
        public int? IdEspecialidad { get; set; }

        // Nombre del servicio
        public string Nombre { get; set; } = string.Empty;

        // Descripción del servicio
        public string? Descripcion { get; set; }

        // Imagen representativa
        public string? ImagenRepresentativa { get; set; }

        // Precio del servicio
        public decimal Costo { get; set; }

        // Disponibilidad del servicio
        public bool? Disponible { get; set; }
    }
}
