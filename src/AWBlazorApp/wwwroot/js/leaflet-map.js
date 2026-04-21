// Thin Leaflet wrapper used by /analytics/geo. Leaflet + MarkerCluster are loaded via CDN
// from App.razor. Keeps the map instance per elementId so we can re-render markers without
// tearing down the map (Blazor re-renders happen on every filter change).
(function () {
    const instances = new Map();   // elementId -> { map, cluster }

    // Mirrors ChartPalettes.BlueScale (see src/AWBlazorApp/Shared/Theming/ChartPalettes.cs).
    // Keep in sync when the chart palette changes so map markers match the rest of the app.
    const territoryColors = [
        '#1F6FEB', '#475569', '#0B3D91', '#94A3B8', '#4A90E2',
        '#1F2937', '#77B0F2', '#64748B', '#93C5FD', '#0F172A',
    ];

    // Poll for Leaflet (loaded from unpkg). In Blazor Server, OnAfterRenderAsync(firstRender)
    // can fire before async CDN scripts finish parsing, so naive calls to render() no-op.
    // Retries every 50ms for up to ~5s before giving up.
    function waitForLeaflet(timeoutMs) {
        return new Promise((resolve, reject) => {
            if (window.L) return resolve();
            const started = Date.now();
            const t = setInterval(() => {
                if (window.L) { clearInterval(t); resolve(); }
                else if (Date.now() - started > timeoutMs) {
                    clearInterval(t);
                    reject(new Error('Leaflet failed to load within ' + timeoutMs + 'ms'));
                }
            }, 50);
        });
    }

    window.awLeafletMap = {
        async render(elementId, markers) {
            await waitForLeaflet(5000);

            const container = document.getElementById(elementId);
            if (!container) {
                console.warn('awLeafletMap: element not found: ' + elementId);
                return;
            }

            let inst = instances.get(elementId);
            if (!inst) {
                const map = L.map(elementId, { preferCanvas: true }).setView([20, 0], 2);
                L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    maxZoom: 18,
                    attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
                }).addTo(map);
                const cluster = L.markerClusterGroup ? L.markerClusterGroup() : L.layerGroup();
                cluster.addTo(map);
                inst = { map, cluster };
                instances.set(elementId, inst);
            } else {
                inst.cluster.clearLayers();
            }

            // Blazor's SPA-style navigation means the page container may have been laid out
            // before Leaflet added its tiles. Tell Leaflet to recompute dimensions.
            setTimeout(() => inst.map.invalidateSize(), 0);

            if (!markers || markers.length === 0) {
                inst.map.setView([20, 0], 2);
                return;
            }

            const bounds = [];
            markers.forEach(m => {
                const color = territoryColors[(m.territoryId - 1) % territoryColors.length];
                const marker = L.circleMarker([m.latitude, m.longitude], {
                    radius: 5,
                    color: color,
                    fillColor: color,
                    fillOpacity: 0.85,
                    weight: 1,
                });
                marker.bindPopup(
                    `<strong>${escapeHtml(m.city)}</strong>, ${escapeHtml(m.stateProvince)}<br>` +
                    `<em>${escapeHtml(m.territoryName)}</em>`
                );
                inst.cluster.addLayer(marker);
                bounds.push([m.latitude, m.longitude]);
            });

            if (bounds.length > 0) {
                inst.map.fitBounds(bounds, { padding: [32, 32], maxZoom: 6 });
            }
        },

        dispose(elementId) {
            const inst = instances.get(elementId);
            if (!inst) return;
            inst.map.remove();
            instances.delete(elementId);
        },
    };

    function escapeHtml(s) {
        return String(s ?? '').replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;',
        }[c]));
    }
})();
