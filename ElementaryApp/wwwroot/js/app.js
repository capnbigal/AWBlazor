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
