
using Lavillemvc.DAL;
using Lavillemvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Npgsql;

namespace Lavillemvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly CuentaRepositorio _cuentas;

        private readonly PasswordHasher<CuentaUsuario>
            _passwordHasher = new();

        public LoginController(CuentaRepositorio cuentas)
        {
            _cuentas = cuentas;
        }

        // ==========================================
        // 1. INICIAR SESIÓN - GET
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ==========================================
        // 2. INICIAR SESIÓN - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(
            string txtEmail,
            string txtPassword,
            bool chkRecordarme = false)
        {
            if (string.IsNullOrWhiteSpace(txtEmail) ||
                string.IsNullOrWhiteSpace(txtPassword))
            {
                ViewBag.Error =
                    "Debes ingresar tu correo y contraseña.";

                return View();
            }

            string correo =
                txtEmail.Trim().ToLowerInvariant();

            CuentaUsuario? cuenta;

            try
            {
                cuenta = await _cuentas.BuscarPorCorreoAsync(
                    correo
                );
            }
            catch (Exception)
            {
                ViewBag.Error =
                    "No fue posible consultar tu cuenta.";

                return View();
            }

            if (cuenta == null ||
                string.IsNullOrWhiteSpace(cuenta.PasswordHash))
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

                return View();
            }

            PasswordVerificationResult resultado;

            try
            {
                resultado =
                    _passwordHasher.VerifyHashedPassword(
                        cuenta,
                        cuenta.PasswordHash,
                        txtPassword
                    );
            }
            catch (FormatException)
            {
                ViewBag.Error =
                    "Esta cuenta necesita restablecer su contraseña.";

                return View();
            }
            catch (ArgumentException)
            {
                ViewBag.Error =
                    "Esta cuenta necesita restablecer su contraseña.";

                return View();
            }

            if (resultado ==
                PasswordVerificationResult.Failed)
            {
                ViewBag.Error =
                    "Correo o contraseña incorrectos.";

                return View();
            }

            // ==========================================
            // 3. IDENTIFICAR ROL
            // ==========================================

            string rol = cuenta.TipoCuenta == "cliente"
                ? "Cliente"
                : cuenta.Cargo?.Trim() ?? "";

            string[] rolesPermitidos =
            {
                "Cliente",
                "Recepcionista",
                "Especialista",
                "Encargado Inventario"
            };

            string? rolValido = rolesPermitidos
                .FirstOrDefault(r =>
                    string.Equals(
                        r,
                        rol,
                        StringComparison.OrdinalIgnoreCase
                    ));

            if (rolValido == null)
            {
                ViewBag.Error =
                    "Tu cuenta no tiene un rol habilitado.";

                return View();
            }

            // ==========================================
            // 4. CREAR SESIÓN AUTENTICADA
            // ==========================================

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    $"{cuenta.TipoCuenta}:{cuenta.IdUsuario}"
                ),

                new Claim(
                    ClaimTypes.Name,
                    cuenta.NombreCompleto
                ),

                new Claim(
                    ClaimTypes.Email,
                    cuenta.Correo
                ),

                new Claim(
                    ClaimTypes.Role,
                    rolValido
                ),

                new Claim(
                    "TipoCuenta",
                    cuenta.TipoCuenta
                )
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme
            );

            var principal = new ClaimsPrincipal(identity);

            var propiedades =
                new AuthenticationProperties
                {
                    IsPersistent = chkRecordarme,
                    AllowRefresh = true
                };

            if (chkRecordarme)
            {
                propiedades.ExpiresUtc =
                    DateTimeOffset.UtcNow.AddMinutes(45);
            }

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal,
                propiedades
            );

            // ==========================================
            // 5. REDIRIGIR A INICIO TRAS AUTENTICARSE
            // El botón "Ir al panel" dirige según el rol.
            // ==========================================

            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // 6. REGISTRO - GET
        // ==========================================

        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // ==========================================
        // 7. REGISTRO - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(
            string txtNombre,
            string txtEmail,
            string? txtTelefono,
            string txtPassword,
            string txtConfirmPassword,
            bool chkTerminos = false)
        {
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(txtNombre) ||
                string.IsNullOrWhiteSpace(txtEmail) ||
                string.IsNullOrWhiteSpace(txtPassword) ||
                string.IsNullOrWhiteSpace(txtConfirmPassword))
            {
                ViewBag.Error =
                    "Completa todos los campos obligatorios.";

                return View();
            }

            // Validar formato de correo
            try
            {
                var direccion =
                    new System.Net.Mail.MailAddress(
                        txtEmail.Trim()
                    );

                if (direccion.Address != txtEmail.Trim())
                {
                    ViewBag.Error =
                        "Ingresa un correo electrónico válido.";

                    return View();
                }
            }
            catch (FormatException)
            {
                ViewBag.Error =
                    "Ingresa un correo electrónico válido.";

                return View();
            }

            // Confirmar contraseñas
            if (txtPassword != txtConfirmPassword)
            {
                ViewBag.Error =
                    "Las contraseñas no coinciden.";

                return View();
            }

            // Longitud mínima
            if (txtPassword.Length < 8)
            {
                ViewBag.Error =
                    "La contraseña debe tener al menos 8 caracteres.";

                return View();
            }

            // Validar aceptación de términos
            if (!chkTerminos)
            {
                ViewBag.Error =
                    "Debes aceptar los términos y condiciones.";

                return View();
            }

            try
            {
                var resultado =
                    await _cuentas.RegistrarClienteAsync(
                        txtNombre,
                        txtEmail,
                        txtTelefono,
                        txtPassword
                    );

                if (!resultado.Exito)
                {
                    ViewBag.Error = resultado.Mensaje;
                    return View();
                }

                TempData["RegistroExitoso"] =
                    "Tu cuenta fue creada correctamente. " +
                    "Ahora puedes iniciar sesión.";

                return RedirectToAction(
                    "Index",
                    "Login"
                );
            }


            catch (Npgsql.PostgresException ex)
            {
                Console.WriteLine("========== ERROR POSTGRESQL ==========");
                Console.WriteLine("Código SQLSTATE: " + ex.SqlState);
                Console.WriteLine("Mensaje: " + ex.MessageText);
                Console.WriteLine("Tabla: " + ex.TableName);
                Console.WriteLine("Columna: " + ex.ColumnName);
                Console.WriteLine("Restricción: " + ex.ConstraintName);
                Console.WriteLine("======================================");

                ViewBag.Error =
                    "No fue posible registrar la cuenta. " +
                    "Estamos revisando un problema con la base de datos.";

                return View();
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "========== ERROR DE REGISTRO ==========");

                System.Diagnostics.Debug.WriteLine(
                    "TIPO: " + ex.GetType().FullName);

                if (ex is Npgsql.PostgresException errorSql)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "SQLSTATE: " + errorSql.SqlState);

                    System.Diagnostics.Debug.WriteLine(
                        "MENSAJE: " + errorSql.MessageText);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(
                        "MENSAJE: " + ex.Message);
                }

                System.Diagnostics.Debug.WriteLine(
                    "=======================================");

                ViewBag.Error =
                    "Ocurrió un problema al registrar tu cuenta. " +
                    "Intenta nuevamente.";

                return View();
            }



        }


        // ==========================================
        // 8. CERRAR SESIÓN
        // ==========================================

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme
            );

            return RedirectToAction(
                "Index",
                "Login"
            );
        }

        // ==========================================
        // 9. ACCESO DENEGADO
        // ==========================================

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return Content(
                "No tienes permisos para acceder a esta sección."
            );
        }
    }
}
