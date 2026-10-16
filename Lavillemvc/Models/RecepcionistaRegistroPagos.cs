
using System;
using System.Collections.Generic;

namespace Lavillemvc.Models
{
    public class RecepcionistaRegistroPagos
    {
        // =====================================
        // INFORMACIÓN DE SUCURSAL
        // =====================================

        public int IdSucursal { get; set; }

        public string NombreSucursal { get; set; }
            = string.Empty;


        // =====================================
        // PERÍODO Y MONEDA
        // =====================================

        public DateOnly MesReferencia { get; set; }

        public string Periodo { get; set; }
            = string.Empty;

        public DateOnly FechaHoy { get; set; }

        public string Moneda { get; set; }
            = "USD";


        // =====================================
        // INDICADORES PRINCIPALES
        // =====================================

        public decimal IngresosDia { get; set; }

        public decimal IngresosMes { get; set; }

        public int PagosPendientes { get; set; }

        public int TransaccionesHoy { get; set; }

        public int TotalTransaccionesMes { get; set; }


        // =====================================
        // DATOS JSON DE SUPABASE
        // =====================================

        public string TransaccionesJson { get; set; }
            = "[]";

        public string MetodosPagoJson { get; set; }
            = "[]";


        // =====================================
        // LISTAS PARA LA INTERFAZ
        // =====================================

        public List<TransaccionPago> Transacciones
        { get; set; } = new();

        public List<MetodoPagoResumen> MetodosPago
        { get; set; } = new();


        // =====================================
        // CLASE INTERNA: TRANSACCIÓN
        // =====================================

        public class TransaccionPago
        {
            public int IdPago { get; set; }

            public int? IdFactura { get; set; }

            public string? NumeroFactura { get; set; }

            public int IdCita { get; set; }

            public int IdCliente { get; set; }

            public string? Especialista { get; set; }

            public decimal Monto { get; set; }

            public string? MetodoPago { get; set; }

            public DateOnly Fecha { get; set; }

            public string? EstadoPago { get; set; }

            public bool EsCobroConFecha { get; set; }
        }


        // =====================================
        // CLASE INTERNA: MÉTODO DE PAGO
        // =====================================

        public class MetodoPagoResumen
        {
            public string Metodo { get; set; }
                = string.Empty;

            public decimal Monto { get; set; }

            public int Cantidad { get; set; }

            public decimal Porcentaje { get; set; }
        }
    }
}
