// STDMS client helpers: password visibility toggles, password strength
// meter, and auto-dismissing flash alerts. Vanilla JS, no dependencies.
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        // ---- Show / hide password (eye toggle) --------------------------
        document.querySelectorAll("[data-password-toggle]").forEach(function (button) {
            button.addEventListener("click", function () {
                var wrap = button.closest(".password-wrap");
                if (!wrap) return;
                var input = wrap.querySelector("input");
                if (!input) return;

                var showing = input.type === "text";
                input.type = showing ? "password" : "text";
                button.setAttribute("aria-pressed", String(!showing));
                button.setAttribute("aria-label", showing ? "Show password" : "Hide password");
                button.setAttribute("title", showing ? "Show password" : "Hide password");

                var icon = button.querySelector("i");
                if (icon) {
                    icon.classList.toggle("fa-eye", showing);
                    icon.classList.toggle("fa-eye-slash", !showing);
                }
                input.focus();
            });
        });

        // ---- Password strength meter ------------------------------------
        document.querySelectorAll("[data-strength-input]").forEach(function (input) {
            var meter = document.querySelector("[data-strength-meter]");
            if (!meter) return;
            var bar = meter.querySelector(".password-strength-bar");
            var text = meter.querySelector("[data-strength-text]");

            function score(value) {
                var points = 0;
                if (value.length >= 8) points++;
                if (value.length >= 12) points++;
                if (/[a-z]/.test(value) && /[A-Z]/.test(value)) points++;
                if (/\d/.test(value)) points++;
                if (/[^A-Za-z0-9]/.test(value)) points++;
                return points; // 0..5
            }

            input.addEventListener("input", function () {
                var value = input.value;
                if (!value) {
                    meter.hidden = true;
                    return;
                }
                meter.hidden = false;
                var points = score(value);
                var levels = [
                    { label: "Too weak", width: "15%", color: "#B3372F" },
                    { label: "Weak", width: "30%", color: "#B3372F" },
                    { label: "Fair", width: "50%", color: "#C08A2D" },
                    { label: "Good", width: "70%", color: "#7A9A3D" },
                    { label: "Strong", width: "88%", color: "#1E5B44" },
                    { label: "Excellent", width: "100%", color: "#1E5B44" }
                ];
                var level = levels[points];
                if (bar) {
                    bar.style.width = level.width;
                    bar.style.backgroundColor = level.color;
                }
                if (text) {
                    text.textContent = "Password strength: " + level.label;
                    text.style.color = level.color;
                }
            });
        });

        // ---- Auto-dismiss flash alerts ----------------------------------
        document.querySelectorAll(".alert.alert-success, .alert.alert-info").forEach(function (alert) {
            setTimeout(function () {
                alert.style.transition = "opacity .45s ease";
                alert.style.opacity = "0";
                setTimeout(function () { alert.remove(); }, 500);
            }, 6000);
        });
    });
})();
