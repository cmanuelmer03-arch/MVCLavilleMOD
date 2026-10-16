(() => {
    "use strict";
    const KEY = "lavielle.accessibility.v1";
    const DEFAULTS = { font: "normal", theme: "light" };
    const VALID_FONT = new Set(["normal", "large", "xlarge"]);
    const VALID_THEME = new Set(["light", "dark"]);
    const root = document.documentElement;

    function readSettings() {
        try {
            const saved = JSON.parse(localStorage.getItem(KEY) || "null");
            return {
                font: VALID_FONT.has(saved?.font) ? saved.font : DEFAULTS.font,
                theme: VALID_THEME.has(saved?.theme) ? saved.theme : DEFAULTS.theme
            };
        } catch {
            return { ...DEFAULTS };
        }
    }

    let settings = readSettings();

    function render() {
        root.dataset.laFont = settings.font;
        root.dataset.laTheme = settings.theme;
        document.querySelectorAll("#la-a11y [data-la-font]").forEach(button => {
            button.setAttribute("aria-pressed", String(button.dataset.laFont === settings.font));
        });
        document.querySelectorAll("#la-a11y [data-la-theme]").forEach(button => {
            button.setAttribute("aria-pressed", String(button.dataset.laTheme === settings.theme));
        });
    }

    function save() {
        try { localStorage.setItem(KEY, JSON.stringify(settings)); }
        catch { /* Si el navegador bloquea almacenamiento, la página sigue funcionando. */ }
    }

    // Aplica preferencias antes de esperar al evento DOMContentLoaded.
    render();

    document.addEventListener("DOMContentLoaded", () => {
        const widget = document.getElementById("la-a11y");
        if (!widget) return;
        const details = widget.querySelector("details");
        render(); // Sincroniza aria-pressed con las preferencias guardadas.

        widget.addEventListener("click", event => {
            const button = event.target.closest("button");
            if (!button) return;
            if (VALID_FONT.has(button.dataset.laFont)) settings.font = button.dataset.laFont;
            else if (VALID_THEME.has(button.dataset.laTheme)) settings.theme = button.dataset.laTheme;
            else if (button.hasAttribute("data-la-reset")) settings = { ...DEFAULTS };
            else return;
            render();
            save();
        });

        document.addEventListener("pointerdown", event => {
            if (details?.open && !widget.contains(event.target)) details.open = false;
        });
        document.addEventListener("keydown", event => {
            if (event.key === "Escape" && details?.open) {
                details.open = false;
                details.querySelector("summary")?.focus();
            }
        });
    });
})();
