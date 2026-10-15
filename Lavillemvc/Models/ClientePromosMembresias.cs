
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class ClientePromosMembresias
    {
        // =====================================
        // INFORMACIÓN DEL CLIENTE
        // =====================================

        public int IdCliente { get; set; }

        public string NombreCompleto { get; set; }
            = string.Empty;

        public string? TipoCliente { get; set; }

        public int PuntosFidelidad { get; set; }

        public DateOnly FechaReferencia { get; set; }


        // =====================================
        // MEMBRESÍA ACTUAL
        // =====================================

        public int? IdMembresiaActual { get; set; }

        public string? MembresiaActual { get; set; }

        public string? BeneficiosMembresiaActual
        { get; set; }

        public string? DuracionMembresiaActual
        { get; set; }

        public DateTime? FechaInicioMembresia
        { get; set; }

        public DateTime? FechaFinMembresia
        { get; set; }

        public string? EstadoMembresia { get; set; }


        // =====================================
        // INDICADORES
        // =====================================

        public int TotalPromocionesActivas
        { get; set; }

        public int TotalPlanes { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string PromocionesActivasJson
        { get; set; } = "[]";

        public string OfertasTemporadaJson
        { get; set; } = "[]";

        public string PlanesMembresiaJson
        { get; set; } = "[]";


        // =====================================
        // LISTAS PARA MOSTRAR EN LA PÁGINA
        // =====================================

        public List<PromocionCliente> PromocionesActivas
        { get; set; } = new();

        public List<PromocionCliente> OfertasTemporada
        { get; set; } = new();

        public List<PlanMembresia> PlanesMembresia
        { get; set; } = new();


        // =====================================
        // INFORMACIÓN DE UNA PROMOCIÓN
        // =====================================

        public class PromocionCliente
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

            public decimal? PrecioEspecial
            { get; set; }

            public string? ImagenUrl { get; set; }

            public List<string> ServiciosRelacionados
            { get; set; } = new();
        }


        // =====================================
        // INFORMACIÓN DE UN PLAN
        // =====================================

        public class PlanMembresia
        {
            public int IdMembresia { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Beneficios { get; set; }

            public string? Duracion { get; set; }

            public decimal? Precio { get; set; }

            public bool? EsRecomendado { get; set; }
        }
    }
}
