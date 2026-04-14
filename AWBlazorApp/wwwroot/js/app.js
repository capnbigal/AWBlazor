// Global keyboard shortcut: Ctrl+K / Cmd+K focuses the search input.
(function () {
    document.addEventListener('keydown', function (e) {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            var input = document.querySelector('.global-search-input input');
            if (input) input.focus();
        }
    });
})();

// Dark mode cookie helper — called from Blazor via JSInterop.
// getDarkModeCookie() is defined inline in App.razor <head> so it's available before the circuit connects.
window.setDarkModeCookie = function(isDark) {
    var value = isDark ? 'true' : 'false';
    var expires = new Date();
    expires.setFullYear(expires.getFullYear() + 1);
    document.cookie = 'darkMode=' + value + ';path=/;expires=' + expires.toUTCString() + ';samesite=lax';
    if (isDark) {
        document.documentElement.setAttribute('data-mud-theme', 'dark');
    } else {
        document.documentElement.removeAttribute('data-mud-theme');
    }
};
