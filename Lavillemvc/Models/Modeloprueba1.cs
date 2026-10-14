
namespace Lavillemvc.Models
{
    public class Modeloprueba1
    {
        public int IdCita { get; set; }

        public string Cliente { get; set; }

        public string TelefonoCliente { get; set; }

        public string Servicio { get; set; }

        public string Especialidad { get; set; }

        public decimal PrecioServicio { get; set; }

        public string Especialista { get; set; }

        public string EmpleadoAsignado { get; set; }

        public string Sucursal { get; set; }

        public DateTime FechaCita { get; set; }

        public TimeSpan HoraCita { get; set; }

        public string EstadoCita { get; set; }
    }
}
