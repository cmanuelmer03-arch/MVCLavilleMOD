
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Lavillemvc.Models
{
    public class AdminGestionServicios
    {
        // Listas de la pantalla administrativa

        public List<AdminServicioItem> Servicios { get; set; }
            = new List<AdminServicioItem>();

        public List<AdminMembresiaItem> Membresias { get; set; }
            = new List<AdminMembresiaItem>();

        // Estadísticas generales

        public int TotalServicios
        {
            get { return Servicios.Count; }
        }

        public int ServiciosActivos
        {
            get
            {
                return Servicios.FindAll(
                    s => s.Disponible == true).Count;
            }
        }

        public int ServiciosInactivos
        {
            get
            {
                return Servicios.FindAll(
                    s => s.Disponible != true).Count;
            }
        }

        public int TotalMembresias
        {
            get { return Membresias.Count; }
        }

        public int MembresiasActivas
        {
            get
            {
                return Membresias.FindAll(
                    m => m.Activo == true).Count;
            }
        }
    }

    // =============================================
    // SERVICIOS
    // =============================================

    public class AdminServicioItem
    {
        public int IdServicio { get; set; }

        public int? IdEspecialidad { get; set; }

        public int? IdServicioPadre { get; set; }

        public string Nombre { get; set; } = "";

        public string? Descripcion { get; set; }

        public string? ImagenRepresentativa { get; set; }

        public decimal Costo { get; set; }

        public int? DuracionMinutos { get; set; }

        public bool? Disponible { get; set; }

        public string EstadoServicio { get; set; } = "";

        public int ReservasAsociadas { get; set; }

        public int SucursalesAsociadas { get; set; }

        public int SucursalesDisponibles { get; set; }

        public int PromocionesAsociadas { get; set; }

        // Datos para mostrar en pantalla

        public string PrecioMostrar
        {
            get
            {
                return Costo.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("en-US"));
            }
        }

        public string DuracionMostrar
        {
            get
            {
                if (!DuracionMinutos.HasValue)
                {
                    return "Duración no configurada";
                }

                int horas = DuracionMinutos.Value / 60;
                int minutos = DuracionMinutos.Value % 60;

                if (horas > 0 && minutos > 0)
                {
                    return horas + " h " + minutos + " min";
                }

                if (horas > 0)
                {
                    return horas + " h";
                }

                return minutos + " min";
            }
        }

        public string AccionEstado
        {
            get
            {
                return Disponible == true
                    ? "Desactivar"
                    : "Activar";
            }
        }
    }

    // =============================================
    // MEMBRESÍAS
    // =============================================

    public class AdminMembresiaItem
    {
        public int IdMembresia { get; set; }

        public string Nombre { get; set; } = "";

        public string? Beneficios { get; set; }

        public string? Duracion { get; set; }

        public decimal? PrecioMensual { get; set; }

        public bool? Activo { get; set; }

        public string EstadoMembresia { get; set; } = "";

        public int? MiembrosActivos { get; set; }

        // Datos para mostrar en pantalla

        public string PrecioMostrar
        {
            get
            {
                if (!PrecioMensual.HasValue)
                {
                    return "Precio no configurado";
                }

                return PrecioMensual.Value.ToString(
                    "C2",
                    CultureInfo.GetCultureInfo("en-US")) + "/mes";
            }
        }

        public string MiembrosMostrar
        {
            get
            {
                return MiembrosActivos.HasValue
                    ? MiembrosActivos.Value.ToString()
                    : "Sin datos";
            }
        }

        public string AccionEstado
        {
            get
            {
                return Activo == true
                    ? "Desactivar"
                    : "Activar";
            }
        }
    }
}
