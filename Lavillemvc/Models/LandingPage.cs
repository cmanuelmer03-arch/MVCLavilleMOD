
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class LandingPage
    {
        // =====================================
        // FECHA DE REFERENCIA
        // =====================================

        public DateOnly FechaReferencia { get; set; }


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int TotalServiciosDisponibles { get; set; }

        public int TotalEspecialistasDisponibles { get; set; }

        public int TotalPromocionesVigentes { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string ServiciosJson { get; set; } = "[]";

        public string EspecialistasJson { get; set; } = "[]";

        public string PromocionesJson { get; set; } = "[]";


        // =====================================
        // LISTAS PARA LA VISTA RAZOR
        // =====================================

        public List<ServicioInicio> Servicios =>
            JsonSerializer.Deserialize<List<ServicioInicio>>(
                ServiciosJson ?? "[]"
            ) ?? new List<ServicioInicio>();


        public List<EspecialistaInicio> Especialistas =>
            JsonSerializer.Deserialize<List<EspecialistaInicio>>(
                EspecialistasJson ?? "[]"
            ) ?? new List<EspecialistaInicio>();


        public List<PromocionInicio> Promociones =>
            JsonSerializer.Deserialize<List<PromocionInicio>>(
                PromocionesJson ?? "[]"
            ) ?? new List<PromocionInicio>();


        // =====================================
        // CLASE INTERNA: SERVICIOS
        // =====================================

        public class ServicioInicio
        {
            public int IdServicio { get; set; }

            public int? IdEspecialidad { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Descripcion { get; set; }

            public string? ImagenRepresentativa
            { get; set; }

            public string? NombreEspecialidad
            { get; set; }

            public decimal Costo { get; set; }

            public bool? Disponible { get; set; }
        }


        // =====================================
        // CLASE INTERNA: ESPECIALISTAS
        // =====================================

        public class EspecialistaInicio
        {
            public int IdEspecialista { get; set; }

            public int IdEspecialidad { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? RolEspecialista { get; set; }

            public string? NombreEspecialidad
            { get; set; }

            public int? AniosExperiencia { get; set; }

            public bool? Disponibilidad { get; set; }

            public string? FotoUrl { get; set; }
        }


        // =====================================
        // CLASE INTERNA: PROMOCIONES
        // =====================================

        public class PromocionInicio
        {
            public int IdPromocion { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Descripcion { get; set; }

            public decimal? Descuento { get; set; }

            public DateTime? FechaInicio { get; set; }

            public DateTime? FechaFin { get; set; }

            public int IdSucursal { get; set; }

            public string NombreSucursal { get; set; }
                = string.Empty;

            public string? ImagenUrl { get; set; }

            public List<string> ServiciosRelacionados
            { get; set; } = new List<string>();
        }
    }
}
