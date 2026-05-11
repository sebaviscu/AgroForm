// ==========================================================================
// dark-theme.js — Centralized Dark Mode Manager
// ==========================================================================
// Provides unified theme toggle, persistence, Chart.js integration,
// and DataTables refresh. Must be loaded after Chart.js (if used).
// ==========================================================================

var DarkTheme = (function () {
    'use strict';

    // --- Constants ---
    var STORAGE_KEY = 'theme';
    var THEME_DARK = 'dark';
    var THEME_LIGHT = 'light';
    var ATTR_NAME = 'data-theme';

    // --- Chart.js default colors for dark mode ---
    var CHART_DARK_COLOR = '#adb5bd';
    var CHART_DARK_BORDER = 'rgba(255,255,255,0.08)';

    // --- Internal state ---
    var currentTheme = null;

    // ================================================================
    // Public API
    // ================================================================

    /**
     * Applies the given theme to the document body and persists it.
     * @param {string} theme - 'dark' or 'light'
     */
    function applyTheme(theme) {
        if (theme === THEME_DARK) {
            document.body.setAttribute(ATTR_NAME, THEME_DARK);
        } else {
            document.body.removeAttribute(ATTR_NAME);
        }
        localStorage.setItem(STORAGE_KEY, theme);
        currentTheme = theme;

        // Apply Chart.js dark defaults if Chart is available
        applyChartDefaults(theme);

        // Notify other scripts
        document.dispatchEvent(new CustomEvent('themeChanged', { detail: { theme: theme } }));
    }

    /**
     * Sets the theme (alias for applyTheme).
     * @param {string} theme
     */
    function setTheme(theme) {
        applyTheme(theme);
    }

    /**
     * Returns the current active theme.
     * @returns {string} 'dark' or 'light'
     */
    function getTheme() {
        return currentTheme || THEME_LIGHT;
    }

    /**
     * Toggles between dark and light.
     */
    function toggleTheme() {
        var newTheme = currentTheme === THEME_DARK ? THEME_LIGHT : THEME_DARK;
        applyTheme(newTheme);
    }

    /**
     * Refreshes all known DataTable instances so they re-read CSS variables.
     * Call after theme change if DataTables are present.
     */
    function refreshDataTables() {
        if (typeof $.fn !== 'undefined' && $.fn.DataTable) {
            $.fn.dataTable.tables({ api: true }).iterator('table', function () {
                this.draw();
            });
        }
    }

    // ================================================================
    // Internal helpers
    // ================================================================

    /**
     * Applies Chart.js global dark defaults if Chart is loaded.
     */
    function applyChartDefaults(theme) {
        if (typeof Chart === 'undefined') return;
        if (theme === THEME_DARK) {
            Chart.defaults.color = CHART_DARK_COLOR;
            Chart.defaults.borderColor = CHART_DARK_BORDER;
        } else {
            // Reset to light defaults (Chart.js default)
            Chart.defaults.color = '#666';
            Chart.defaults.borderColor = 'rgba(0,0,0,0.1)';
        }
    }

    // ================================================================
    // Initialization
    // ================================================================

    function init() {
        var savedTheme = localStorage.getItem(STORAGE_KEY) || THEME_LIGHT;
        applyTheme(savedTheme);

        // Listen for storage changes (other tabs)
        window.addEventListener('storage', function (e) {
            if (e.key === STORAGE_KEY && e.newValue !== currentTheme) {
                applyTheme(e.newValue || THEME_LIGHT);
            }
        });

        // Wire up the user theme switch if present
        var themeSwitch = document.getElementById('themeSwitchUser');
        if (themeSwitch) {
            themeSwitch.checked = savedTheme === THEME_DARK;
            themeSwitch.addEventListener('change', function () {
                applyTheme(this.checked ? THEME_DARK : THEME_LIGHT);
                refreshDataTables();
            });
        }

        // Wire up the admin theme switch if present
        var adminSwitch = document.getElementById('themeSwitch');
        if (adminSwitch) {
            adminSwitch.checked = savedTheme === THEME_DARK;
            adminSwitch.addEventListener('change', function () {
                applyTheme(this.checked ? THEME_DARK : THEME_LIGHT);
                refreshDataTables();
            });
        }
    }

    // Apply theme immediately to prevent flash
    (function() {
        var savedTheme = localStorage.getItem(STORAGE_KEY) || THEME_LIGHT;
        if (savedTheme === THEME_DARK) {
            document.documentElement.setAttribute(ATTR_NAME, THEME_DARK);
        }
    })();

    // Auto-initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // ================================================================
    // Public exports
    // ================================================================

    return {
        applyTheme: applyTheme,
        setTheme: setTheme,
        getTheme: getTheme,
        toggleTheme: toggleTheme,
        refreshDataTables: refreshDataTables
    };

})();
