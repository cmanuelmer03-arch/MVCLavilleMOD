
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class AdminDashboardGeneral
    {
        // =====================================
        // FECHA DE REFERENCIA
        // =====================================

        public DateTime FechaReferencia { get; set; }


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int ReservasTotales { get; set; }

        public int UsuariosRegistrados { get; set; }

        public int SucursalesActivas { get; set; }

        public int ServiciosDisponibles { get; set; }

        public int PromocionesActivas { get; set; }

        public decimal IngresosMes { get; set; }


        // =====================================
        // RESERVAS DEL MES
        // =====================================

        public int ReservasMes { get; set; }

        public int ReservasConfirmadasMes { get; set; }

        public int ReservasCompletadasMes { get; set; }

        public int ReservasCanceladasMes { get; set; }

        public int ReservasPendientesMes { get; set; }

        public int ReservasOtrosMes { get; set; }


        // =====================================
        // ESTADÍSTICAS RÁPIDAS
        // =====================================

        public int ClientesNuevosMes { get; set; }

        public decimal TasaRepeticionClientes { get; set; }

        public string? ServicioMasPopular { get; set; }

        public string? SucursalTop { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string RendimientoSucursalesJson
        { get; set; } = "[]";

        public string ReservasPorEstadoJson
        { get; set; } = "[]";

        public string ActividadRecienteJson
        { get; set; } = "[]";


        // =====================================
        // LISTA DE RENDIMIENTO POR SUCURSAL
        // =====================================

        public List<RendimientoSucursal> RendimientoSucursales
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<RendimientoSucursal>>(
                        RendimientoSucursalesJson ?? "[]"
                    ) ?? new List<RendimientoSucursal>();
            }
        }


        // =====================================
        // LISTA DE RESERVAS POR ESTADO
        // =====================================

        public List<ReservaPorEstado> ReservasPorEstado
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<ReservaPorEstado>>(
                        ReservasPorEstadoJson ?? "[]"
                    ) ?? new List<ReservaPorEstado>();
            }
        }


        // =====================================
        // LISTA DE ACTIVIDAD RECIENTE
        // =====================================

        public List<ActividadAdministrativa> ActividadReciente
        {
            get
            {
                return JsonSerializer.Deserialize
                    <List<ActividadAdministrativa>>(
                        ActividadRecienteJson ?? "[]"
                    ) ?? new List<ActividadAdministrativa>();
            }
        }


        // =====================================
        // CLASE INTERNA:
        // RENDIMIENTO POR SUCURSAL
        // =====================================

        public class RendimientoSucursal
        {
            public int IdSucursal { get; set; }

            public string NombreSucursal { get; set; }
                = string.Empty;

            public decimal IngresosMes { get; set; }
        }


        // =====================================
        // CLASE INTERNA:
        // RESERVAS POR ESTADO
        // =====================================

        public class ReservaPorEstado
        {
            public string Estado { get; set; }
                = string.Empty;

            public int Cantidad { get; set; }

            public decimal Porcentaje { get; set; }
        }


        // =====================================
        // CLASE INTERNA:
        // ACTIVIDAD ADMINISTRATIVA
        // =====================================

        public class ActividadAdministrativa
        {
            public long IdActividad { get; set; }

            public string TipoEvento { get; set; }
                = string.Empty;

            public string Descripcion { get; set; }
                = string.Empty;

            public DateTimeOffset FechaEvento { get; set; }
        }
    }
}
