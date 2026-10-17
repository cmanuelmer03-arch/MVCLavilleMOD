/* L'Avielle - Módulo de accesibilidad ÚNICO para todos los layouts MVC.
   Se incorpora con una sola etiqueta <script> al <head> de cada layout.
   No requiere cambiar Controllers, Models, SQL ni las Views existentes. */
(() => {
    "use strict";
    if (window.__lavielleAccesibilidadGlobal) return;
    window.__lavielleAccesibilidadGlobal = true;

    const script = document.currentScript;
    const stylesheetUrl = script?.src
        ? new URL("../css/accesibilidad-global.css", script.src).href
        : "/css/accesibilidad-global.css";
    const stylesheet = document.createElement("link");
    stylesheet.rel = "stylesheet";
    stylesheet.href = stylesheetUrl;
    stylesheet.id = "la-accesibilidad-css";
    if (!document.getElementById(stylesheet.id)) document.head.appendChild(stylesheet);

    // Misma clave que el módulo anterior: mantiene las preferencias guardadas.
    const STORAGE_KEY = "lavielle.accessibility.v1";
    const DEFAULTS = { font: "normal", theme: "light" };
    const FONTS = ["normal", "large", "xlarge"];
    const THEMES = ["light", "dark"];
    const html = document.documentElement;

    function read() {
        try {
            const data = JSON.parse(localStorage.getItem(STORAGE_KEY) || "null");
            return {
                font: FONTS.includes(data?.font) ? data.font : DEFAULTS.font,
                theme: THEMES.includes(data?.theme) ? data.theme : DEFAULTS.theme
            };
        } catch { return { ...DEFAULTS }; }
    }

    let settings = read();

    function render() {
        html.dataset.laTheme = settings.theme;
        html.dataset.laFont = settings.font;
        html.style.colorScheme = settings.theme;
        const widget = document.getElementById("la-a11y");
        if (!widget) return;
        widget.querySelectorAll("button[data-la-font]").forEach(button => {
            button.setAttribute("aria-pressed", String(button.dataset.laFont === settings.font));
        });
        widget.querySelectorAll("button[data-la-theme]").forEach(button => {
            button.setAttribute("aria-pressed", String(button.dataset.laTheme === settings.theme));
        });
    }

    function save() {
        try { localStorage.setItem(STORAGE_KEY, JSON.stringify(settings)); }
        catch { /* Bloquear cookies/almacenamiento no debe romper el menú. */ }
    }

    // Aplica las preferencias antes de pintar la página.
    render();

    function init() {
        if (document.getElementById("la-a11y")) { render(); return; }
        const widget = document.createElement("div");
        widget.id = "la-a11y";
        widget.className = "la-a11y";
        widget.innerHTML = `
            <button type="button" class="la-a11y__trigger"
                    id="la-a11y-open" aria-controls="la-a11y-panel"
                    aria-expanded="false" aria-label="Abrir menú de accesibilidad">
                <span aria-hidden="true">⚙</span> Accesibilidad
            </button>
            <section class="la-a11y__content" id="la-a11y-panel" hidden
                     role="region" aria-label="Opciones de accesibilidad">
                <p class="la-a11y__heading">Personalizar visualización</p>
                <fieldset class="la-a11y__fieldset">
                    <legend>Tamaño del texto</legend>
                    <div class="la-a11y__choices" role="group" aria-label="Tamaño del texto">
                        <button type="button" data-la-font="normal" aria-pressed="true">Normal</button>
                        <button type="button" data-la-font="large" aria-pressed="false">Grande</button>
                        <button type="button" data-la-font="xlarge" aria-pressed="false">Muy grande</button>
                    </div>
                </fieldset>
                <fieldset class="la-a11y__fieldset">
                    <legend>Apariencia</legend>
                    <div class="la-a11y__choices" role="group" aria-label="Apariencia">
                        <button type="button" data-la-theme="light" aria-pressed="true">Claro</button>
                        <button type="button" data-la-theme="dark" aria-pressed="false">Oscuro</button>
                    </div>
                </fieldset>
                <p class="la-a11y__note">Tus preferencias se mantienen al navegar por L’Avielle.</p>
                <button type="button" class="la-a11y__reset" data-la-reset>
                    Restablecer preferencias
                </button>
            </section>`;
        document.body.appendChild(widget);
        const toggle = widget.querySelector("#la-a11y-open");
        const panel = widget.querySelector("#la-a11y-panel");

        function setOpen(open, restoreFocus = false) {
            panel.hidden = !open;
            toggle.setAttribute("aria-expanded", String(open));
            toggle.setAttribute("aria-label", open ? "Cerrar menú de accesibilidad" : "Abrir menú de accesibilidad");
            if (restoreFocus) toggle.focus();
        }

        toggle.addEventListener("click", () => setOpen(panel.hidden));
        widget.addEventListener("click", event => {
            const button = event.target.closest("button");
            if (!button || button === toggle) return;
            if (FONTS.includes(button.dataset.laFont)) settings.font = button.dataset.laFont;
            else if (THEMES.includes(button.dataset.laTheme)) settings.theme = button.dataset.laTheme;
            else if (button.hasAttribute("data-la-reset")) settings = { ...DEFAULTS };
            else return;
            render();
            save();
        });
        document.addEventListener("pointerdown", event => {
            if (!panel.hidden && !widget.contains(event.target)) setOpen(false);
        });
        document.addEventListener("keydown", event => {
            if (event.key === "Escape" && !panel.hidden) setOpen(false, true);
        });
        render();
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", init, { once: true });
    } else { init(); }

    // Si se cambia una preferencia en otra pestaña, esta se sincroniza.
    window.addEventListener("storage", event => {
        if (event.key === STORAGE_KEY) { settings = read(); render(); }
    });
})();
