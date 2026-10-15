
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class ClienteProcesoPago
    {
        // =====================================
        // IDENTIFICACIÓN DE LA CITA
        // =====================================

        public int IdCita { get; set; }

        public int IdCliente { get; set; }

        public string? EstadoReserva { get; set; }


        // =====================================
        // DATOS DEL CLIENTE
        // =====================================

        public string NombreCliente { get; set; }
            = string.Empty;

        public string? CorreoCliente { get; set; }


        // =====================================
        // SERVICIOS CONTRATADOS
        // =====================================

        public int IdServicio { get; set; }

        public string ServicioPrincipal { get; set; }
            = string.Empty;

        public string? ServiciosContratados { get; set; }

        public decimal PrecioBaseServicio { get; set; }

        public decimal? SubtotalServicios { get; set; }

        public string? DuracionEstimada { get; set; }


        // =====================================
        // ESPECIALISTA
        // =====================================

        public int? IdEspecialista { get; set; }

        public string? NombreEspecialista { get; set; }


        // =====================================
        // SUCURSAL
        // =====================================

        public int? IdSucursal { get; set; }

        public string? NombreSucursal { get; set; }

        public string? DireccionSucursal { get; set; }


        // =====================================
        // FECHA Y HORA
        // =====================================

        public DateOnly FechaCita { get; set; }

        public TimeOnly HoraCita { get; set; }


        // =====================================
        // MEMBRESÍA DEL CLIENTE
        // =====================================

        public int? IdMembresia { get; set; }

        public string? NombreMembresia { get; set; }

        public string? BeneficiosMembresia
        { get; set; }

        public DateTime? FechaFinMembresia
        { get; set; }


        // =====================================
        // INFORMACIÓN DEL PAGO
        // =====================================

        public int? IdPago { get; set; }

        public string? MetodoPago { get; set; }

        public string? EstadoPago { get; set; }

        public string Moneda { get; set; } = "USD";

        public decimal Subtotal { get; set; }

        public decimal? DescuentoMembresiaRegistrado
        { get; set; }

        public decimal? DescuentoGeneralRegistrado
        { get; set; }

        public decimal TotalAPagar { get; set; }

        public bool TienePagoRegistrado { get; set; }


        // =====================================
        // PUNTOS DE FIDELIDAD
        // =====================================

        public int PuntosActuales { get; set; }

        public int? PuntosPorGanar { get; set; }


        // =====================================
        // PROMOCIONES
        // =====================================

        public string PromocionesElegiblesJson
        { get; set; } = "[]";


        // =====================================
        // DATOS DEL FORMULARIO
        // No son columnas de la vista SQL
        // =====================================

        public string? MetodoPagoSeleccionado
        { get; set; }

        public string? CodigoPromocional { get; set; }

        public bool AceptaTerminos { get; set; }

        public bool AutorizaCobro { get; set; }


        // =====================================
        // CLASE PARA PROMOCIONES DISPONIBLES
        // =====================================

        public class PromocionAplicable
        {
            public int IdPromocion { get; set; }

            public string Nombre { get; set; }
                = string.Empty;

            public string? Descripcion { get; set; }

            public decimal? Descuento { get; set; }

            public DateTime? FechaInicio { get; set; }

            public DateTime? FechaFin { get; set; }
        }

        public List<PromocionAplicable>
            PromocionesElegibles
        { get; set; } = new();
    }
}
