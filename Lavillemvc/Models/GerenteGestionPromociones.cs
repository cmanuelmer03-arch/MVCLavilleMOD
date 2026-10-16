
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class GerenteGestionPromociones
    {
        // =====================================
        // IDENTIFICADORES
        // =====================================

        public int IdPromocion { get; set; }

        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }
            = string.Empty;

        public bool? SucursalAbierta { get; set; }


        // =====================================
        // INFORMACIÓN DE LA PROMOCIÓN
        // =====================================

        public string Nombre { get; set; }
            = string.Empty;

        public string? Descripcion { get; set; }

        public decimal? Descuento { get; set; }

        public DateTime? FechaInicio { get; set; }

        public DateTime? FechaFin { get; set; }

        public string EstadoPromocion { get; set; }
            = string.Empty;


        // =====================================
        // FECHAS DE REFERENCIA
        // =====================================

        public DateOnly FechaReferencia { get; set; }

        public DateOnly MesReferencia { get; set; }


        // =====================================
        // INFORMACIÓN VISUAL
        // =====================================

        public string? ImagenUrl { get; set; }


        // =====================================
        // SERVICIOS RELACIONADOS
        // =====================================

        public string ServiciosRelacionadosTexto
        { get; set; } = string.Empty;

        public string ServiciosRelacionadosJson
        { get; set; } = "[]";

        public int TotalServiciosRelacionados
        { get; set; }

        public List<ServicioPromocion> ServiciosRelacionados
        { get; set; } = new();


        // =====================================
        // ESTADÍSTICAS DE LA PROMOCIÓN
        // =====================================

        public int? TotalUsos { get; set; }

        public decimal? IngresosGenerados { get; set; }


        // =====================================
        // INDICADORES GENERALES
        // =====================================

        public int TotalPromociones { get; set; }

        public int PromocionesActivas { get; set; }

        public int PromocionesProgramadas { get; set; }

        public int PromocionesFinalizadas { get; set; }


        // =====================================
        // INDICADORES DEL MES
        // =====================================

        public int? ClientesBeneficiadosMes
        { get; set; }

        public decimal? IngresosPromocionesMes
        { get; set; }


        // =====================================
        // CLASE INTERNA: SERVICIOS
        // =====================================

        public class ServicioPromocion
        {
            public int IdServicio { get; set; }

            public string NombreServicio { get; set; }
                = string.Empty;

            public decimal Precio { get; set; }

            public bool? Disponible { get; set; }
        }
    }
}
