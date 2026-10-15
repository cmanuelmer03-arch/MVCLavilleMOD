
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lavillemvc.Models
{
    public class AdminGestionServicios
    {
        // =====================================
        // FECHA DE REFERENCIA
        // =====================================

        public DateOnly FechaReferencia { get; set; }


        // =====================================
        // INDICADORES DE SERVICIOS
        // =====================================

        public int TotalServicios { get; set; }

        public int ServiciosActivos { get; set; }

        public int ServiciosInactivos { get; set; }


        // =====================================
        // INDICADORES DE MEMBRESÍAS
        // =====================================

        public int TotalMembresias { get; set; }

        public int TotalAfiliacionesActivas { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string ServiciosJson { get; set; } = "[]";

        public string MembresiasJson { get; set; } = "[]";


        // =====================================
        // LISTA DE SERVICIOS
        // =====================================

        public List<ServicioItem> Servicios
        {
            get
            {
                return JsonSerializer.Deserialize<
                    List<ServicioItem>
                >(
                    ServiciosJson ?? "[]"
                ) ?? new List<ServicioItem>();
            }
        }


        // =====================================
        // LISTA DE MEMBRESÍAS
        // =====================================

        public List<MembresiaItem> Membresias
        {
            get
            {
                return JsonSerializer.Deserialize<
                    List<MembresiaItem>
                >(
                    MembresiasJson ?? "[]"
                ) ?? new List<MembresiaItem>();
            }
        }


        // =====================================
        // CLASE INTERNA: SERVICIOS
        // =====================================

        public class ServicioItem
        {
            public int IdServicio { get; set; }

            public int? IdEspecialidad { get; set; }

            public string? NombreEspecialidad
            { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Descripcion { get; set; }

            public string? ImagenRepresentativa
            { get; set; }

            public decimal Costo { get; set; }

            public bool? Disponible { get; set; }

            public decimal PrecioMinimo { get; set; }

            public decimal? PrecioMaximo { get; set; }

            public string? DuracionEstimada
            { get; set; }
        }


        // =====================================
        // CLASE INTERNA: MEMBRESÍAS
        // =====================================

        public class MembresiaItem
        {
            public int IdMembresia { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Beneficios { get; set; }

            public string? Duracion { get; set; }

            public int MiembrosActivos { get; set; }

            public int MiembrosHistoricos { get; set; }

            public decimal? Precio { get; set; }

            public bool? EsPopular { get; set; }
        }
    }
}
