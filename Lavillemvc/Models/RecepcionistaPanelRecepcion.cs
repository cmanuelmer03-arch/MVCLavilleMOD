
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class RecepcionistaPanelRecepcion
    {
        // INFORMACIÓN DE SUCURSAL
        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }
            = string.Empty;

        public string Direccion { get; set; }
            = string.Empty;

        public string? NombreRecepcionista { get; set; }

        public DateTime FechaHoy { get; set; }


        // INDICADORES PRINCIPALES
        public int CitasHoy { get; set; }

        public int PendientesHoy { get; set; }

        public int ClientesNuevosHoy { get; set; }

        public decimal PagosHoy { get; set; }


        // ATENCIÓN ACTUAL
        public int? EspecialistasActivos { get; set; }

        public int? EspecialistasTotales { get; set; }

        public int? CabinasOcupadas { get; set; }

        public int? CabinasTotales { get; set; }

        public int? EsperaPromedioMinutos { get; set; }


        // DATOS JSON DE SUPABASE
        public string CitasHoyJson { get; set; } = "[]";

        public string ReservasPendientesJson { get; set; } = "[]";

        public string UltimosRegistrosJson { get; set; } = "[]";


        // LISTA DE CITAS DEL DÍA
        public List<CitaRecepcion> CitasDelDia =>
            JsonSerializer.Deserialize<List<CitaRecepcion>>(
                CitasHoyJson ?? "[]"
            ) ?? new List<CitaRecepcion>();


        // LISTA DE RESERVAS POR CONFIRMAR
        public List<ReservaPendiente> ReservasPorConfirmar =>
            JsonSerializer.Deserialize<List<ReservaPendiente>>(
                ReservasPendientesJson ?? "[]"
            ) ?? new List<ReservaPendiente>();


        // LISTA DE ÚLTIMOS REGISTROS
        public List<RegistroCliente> UltimosRegistros =>
            JsonSerializer.Deserialize<List<RegistroCliente>>(
                UltimosRegistrosJson ?? "[]"
            ) ?? new List<RegistroCliente>();


        // CLASE INTERNA: CITA
        public class CitaRecepcion
        {
            public int IdCita { get; set; }

            public int IdCliente { get; set; }

            public string Cliente { get; set; }
                = string.Empty;

            public string? TipoCliente { get; set; }

            public string Servicio { get; set; }
                = string.Empty;

            public string? Especialista { get; set; }

            public string Hora { get; set; }
                = string.Empty;

            public string? Estado { get; set; }
        }


        // CLASE INTERNA: RESERVA PENDIENTE
        public class ReservaPendiente
        {
            public int IdCita { get; set; }

            public int IdCliente { get; set; }

            public string Cliente { get; set; }
                = string.Empty;

            public string Servicio { get; set; }
                = string.Empty;

            public string Hora { get; set; }
                = string.Empty;

            public string? Estado { get; set; }
        }


        // CLASE INTERNA: REGISTRO CLIENTE
        public class RegistroCliente
        {
            public int IdCliente { get; set; }

            public string NombreCompleto { get; set; }
                = string.Empty;

            public string FechaRegistro { get; set; }
                = string.Empty;
        }
    }
}
