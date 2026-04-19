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

// File download from a base64-encoded payload — called from Blazor via JSInterop. Used by
// the report runner's "Download CSV" button, which already builds the bytes server-side.
window.downloadFile = function (fileName, contentType, base64) {
    var bin = atob(base64);
    var len = bin.length;
    var bytes = new Uint8Array(len);
    for (var i = 0; i < len; i++) bytes[i] = bin.charCodeAt(i);
    var blob = new Blob([bytes], { type: contentType });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
};

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
