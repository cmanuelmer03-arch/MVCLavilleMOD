using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class KpiRecepcion
    {
        public int CitasHoy { get; set; }
        public int Pendientes { get; set; }
        public int ClientesNuevos { get; set; }
        public decimal PagosDelDia { get; set; }
    }

    public class CitaPanel
    {
        public int IdCita { get; set; }
        public string Hora { get; set; } = "";
        public string Cliente { get; set; } = "";
        public string TipoCliente { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Especialista { get; set; } = "";
        public string Estado { get; set; } = "";
        public bool MostrarConfirmar { get; set; }
    }

    public class ReservaPendiente
    {
        public int IdCita { get; set; }
        public string Nombre { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Fecha { get; set; } = "";
    }

    public class ClienteReciente
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = "";
        public string Tiempo { get; set; } = "";
    }

    public class AtencionResumen
    {
        public int EspecialistasActivos { get; set; }
        public int EspecialistasTotal { get; set; }
        public int CabinasOcupadas { get; set; }
        public int CabinasTotal { get; set; }
        public int EsperaPromedioMin { get; set; }
    }

    public class CitaCalendario
    {
        public int IdCita { get; set; }
        public DateTime Fecha { get; set; }
        public TimeOnly Hora { get; set; }
        public string Cliente { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Especialista { get; set; } = "";
        public string Estado { get; set; } = "";
    }

    public class CitaDetalle
    {
        public int IdCita { get; set; }
        public string Cliente { get; set; } = "";
        public string Iniciales { get; set; } = "";
        public string Membresia { get; set; } = "";
        public string Servicio { get; set; } = "";
        public string Especialista { get; set; } = "";
        public string HorarioTexto { get; set; } = "";
        public string Estado { get; set; } = "";
    }

    public class ClienteDirectorio
    {
        public int IdCliente { get; set; }
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string UltimaVisita { get; set; } = "";
        public int Citas { get; set; }
        public string Membresia { get; set; } = "—";
        public string CssMembresia { get; set; } = "badge-none";
    }

    public class PagoTransaccion
    {
        public int IdFactura { get; set; }
        public string Factura { get; set; } = "";
        public string Especialista { get; set; } = "";
        public string Monto { get; set; } = "";
        public string Metodo { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Estado { get; set; } = "";
        public string BadgeClass { get; set; } = "";
    }

    public class KpiPagos
    {
        public decimal IngresosDia { get; set; }
        public decimal IngresosMes { get; set; }
        public int Pendientes { get; set; }
        public int TransaccionesHoy { get; set; }
    }

    public class DesgloseMetodo
    {
        public string Metodo { get; set; } = "";
        public decimal Monto { get; set; }
        public decimal Porcentaje { get; set; }
        public string ColorHex { get; set; } = "#c8a96a";
    }
}