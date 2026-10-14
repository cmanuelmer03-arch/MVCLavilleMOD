
namespace Lavillemvc.Models
{
    public class GerenteDashboardSucursal
    {
        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public bool? Abierto { get; set; }

        public int ReservasHoy { get; set; }

        public int CitasConfirmadas { get; set; }

        public int CitasPendientes { get; set; }

        public decimal IngresosCitasMes { get; set; }

        public int ClientesMes { get; set; }

        public int TotalEmpleados { get; set; }

        public int PromocionesActivas { get; set; }

        public int ServiciosDisponibles { get; set; }
    }
}
