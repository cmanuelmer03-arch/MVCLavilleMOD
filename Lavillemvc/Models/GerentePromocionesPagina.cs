using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Lavillemvc.Models
{
    // Modelo de apoyo para la página. NO sustituye GerenteGestionPromociones.
    public class GerentePromocionesPagina
    {
        public int IdSucursal { get; set; }
        public string NombreSucursal { get; set; } = string.Empty;
        public List<PromocionGerenteFila> Promociones { get; set; } = new();
        public bool PuedeEditar { get; set; }
        public string? MensajeError { get; set; }

        public int TotalActivas => Promociones.Count(p => p.Estado == "Activa");
        public int TotalProgramadas => Promociones.Count(p => p.Estado == "Programada");
        public int TotalFinalizadas => Promociones.Count(p => p.Estado == "Finalizada");
        public int TotalPausadas => Promociones.Count(p => p.Estado == "Pausada");
        public int TotalArchivadas => Promociones.Count(p => p.Estado == "Archivada");
        public int? ClientesBeneficiadosMes => Promociones.FirstOrDefault()?.Datos.ClientesBeneficiadosMes;
        public decimal? IngresosPromocionesMes => Promociones.FirstOrDefault()?.Datos.IngresosPromocionesMes;
    }

    public class PromocionGerenteFila
    {
        public GerenteGestionPromociones Datos { get; set; } = new();
        public bool Pausada { get; set; }
        public bool Archivada { get; set; }
        public string? ImagenPersonalizada { get; set; }

        public string Estado => Archivada ? "Archivada"
            : Pausada ? "Pausada"
            : Datos.EstadoPromocion;

        // Imágenes locales de presentación: se usarán solo si existen los archivos.
        // No sustituyen la falta de una imagen guardada en Supabase.
        public string? ImagenParaMostrar
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ImagenPersonalizada)) return ImagenPersonalizada;
                if (!string.IsNullOrWhiteSpace(Datos.ImagenUrl)) return Datos.ImagenUrl;
                var nombre = Datos.Nombre.ToLowerInvariant();
                if (nombre.Contains("primera visita")) return "/images/promo-visita.jpg";
                if (nombre.Contains("novia")) return "/images/promo-pack.jpg";
                if (nombre.Contains("martes de color")) return "/images/promo-color.jpg";
                if (nombre.Contains("otoño")) return "/images/promo-otono.jpg";
                if (nombre.Contains("black friday")) return "/images/promo-blackfriday.jpg";
                return null;
            }
        }
    }

    public class GerentePromocionFormulario
    {
        public int? IdPromocion { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Descripcion { get; set; }

        [Range(typeof(decimal), "0", "100", ErrorMessage = "El descuento debe estar entre 0 y 100")]
        public decimal? Descuento { get; set; }

        [Required(ErrorMessage = "Indica la fecha de inicio")]
        public DateTime? FechaInicio { get; set; }

        [Required(ErrorMessage = "Indica la fecha de fin")]
        public DateTime? FechaFin { get; set; }

        // Atributo auxiliar guardado sin alterar la tabla promocion.
        [StringLength(500)]
        public string? ImagenUrl { get; set; }
    }
}
