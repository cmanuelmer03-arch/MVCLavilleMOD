(() => {
    "use strict";
    const buscador = document.getElementById("buscar-promociones");
    const filtros = Array.from(document.querySelectorAll("[data-filtro-promo]"));
    const secciones = Array.from(document.querySelectorAll("[data-seccion-promo]"));
    const aviso = document.getElementById("promo-sin-resultados");
    let filtroActual = "Todas";

    function aplicarFiltros() {
        const termino = (buscador?.value ?? "").trim().toLocaleLowerCase("es");
        let visibles = 0;
        for (const seccion of secciones) {
            const estadoSeccion = seccion.dataset.seccionPromo;
            const mostrarEstado = filtroActual === "Todas" || filtroActual === estadoSeccion;
            let visiblesSeccion = 0;
            for (const item of seccion.querySelectorAll(".promo-item")) {
                const texto = (item.dataset.busqueda ?? "").toLocaleLowerCase("es");
                const mostrar = mostrarEstado && texto.includes(termino);
                item.hidden = !mostrar;
                if (mostrar) visiblesSeccion++;
            }
            // No ocultar el grupo cuando no hay búsqueda, pues contiene su mensaje vacío.
            seccion.hidden = !mostrarEstado || (termino.length > 0 && visiblesSeccion === 0);
            visibles += visiblesSeccion;
        }
        if (aviso) aviso.hidden = visibles > 0 || secciones.every(s => s.hidden === false && !termino);
    }

    filtros.forEach(boton => {
        boton.addEventListener("click", () => {
            filtroActual = boton.dataset.filtroPromo ?? "Todas";
            filtros.forEach(b => {
                const activo = b === boton;
                b.classList.toggle("activo", activo);
                b.setAttribute("aria-pressed", activo ? "true" : "false");
            });
            aplicarFiltros();
        });
    });
    buscador?.addEventListener("input", aplicarFiltros);
    aplicarFiltros();

    document.querySelectorAll("form[data-confirm]").forEach(form => {
        form.addEventListener("submit", e => {
            if (!window.confirm(form.dataset.confirm ?? "¿Confirmar esta acción?")) e.preventDefault();
        });
    });

    const dialogo = document.getElementById("dialogo-promocion");
    const form = document.getElementById("form-promocion");
    if (!(dialogo instanceof HTMLDialogElement) || !form) return;
    const campo = id => document.getElementById(id);
    const titulo = campo("titulo-dialogo-promocion");

    function abrirNuevo() {
        form.reset();
        campo("promocion-id").value = "";
        titulo.textContent = "Crear Nueva Promoción";
        dialogo.showModal();
        campo("promocion-nombre").focus();
    }
    document.getElementById("abrir-nueva-promo")?.addEventListener("click", abrirNuevo);

    document.querySelectorAll(".promo-editar").forEach(boton => {
        boton.addEventListener("click", () => {
            const d = boton.dataset;
            form.reset();
            campo("promocion-id").value = d.id ?? "";
            campo("promocion-nombre").value = d.nombre ?? "";
            campo("promocion-descripcion").value = d.descripcion ?? "";
            campo("promocion-descuento").value = d.descuento ?? "";
            campo("promocion-inicio").value = d.inicio ?? "";
            campo("promocion-fin").value = d.fin ?? "";
            campo("promocion-imagen").value = d.imagen ?? "";
            titulo.textContent = "Editar Promoción";
            dialogo.showModal();
            campo("promocion-nombre").focus();
        });
    });
    campo("cerrar-dialogo-promo")?.addEventListener("click", () => dialogo.close());
    campo("cancelar-dialogo-promo")?.addEventListener("click", () => dialogo.close());
    dialogo.addEventListener("click", e => {
        if (e.target === dialogo) dialogo.close();
    });
    form.addEventListener("submit", e => {
        const inicio = campo("promocion-inicio").value;
        const fin = campo("promocion-fin").value;
        if (inicio && fin && fin < inicio) {
            e.preventDefault();
            window.alert("La fecha de finalización no puede ser anterior a la fecha de inicio.");
        }
    });
})();
