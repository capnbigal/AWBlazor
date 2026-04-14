// Simple fetch helper for the API Explorer page. Using JS fetch (rather than a server-side
// HttpClient) means the browser attaches the user's auth cookie automatically — no need to
// replicate cookie forwarding on the server.
window.awApiExplorer = {
    async send(method, url) {
        const started = performance.now();
        try {
            const res = await fetch(url, {
                method,
                credentials: 'same-origin',
                headers: { 'Accept': 'application/json' },
            });
            const elapsedMs = Math.round(performance.now() - started);
            const ct = res.headers.get('content-type') || '';
            const bodyText = await res.text();

            let body = bodyText;
            let parsed = null;
            if (ct.includes('application/json') && bodyText.length > 0) {
                try { parsed = JSON.parse(bodyText); body = JSON.stringify(parsed, null, 2); }
                catch { /* leave as text */ }
            }

            return {
                ok: res.ok,
                status: res.status,
                statusText: res.statusText,
                elapsedMs,
                contentType: ct,
                body,
                parsed,
            };
        } catch (err) {
            return {
                ok: false,
                status: 0,
                statusText: 'Network error',
                elapsedMs: Math.round(performance.now() - started),
                contentType: '',
                body: String(err?.message ?? err),
                parsed: null,
            };
        }
    },
};
