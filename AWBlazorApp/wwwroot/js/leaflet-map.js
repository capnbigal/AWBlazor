// Thin Leaflet wrapper used by /analytics/geo. Leaflet + MarkerCluster are loaded via CDN
// from App.razor. Keeps the map instance per elementId so we can re-render markers without
// tearing down the map (Blazor re-renders happen on every filter change).
(function () {
    const instances = new Map();   // elementId -> { map, cluster }

    // Palette picked to be visible in both light and dark themes.
    const territoryColors = [
        '#e53935', '#1e88e5', '#43a047', '#8e24aa', '#fb8c00',
        '#3949ab', '#00acc1', '#c0ca33', '#6d4c41', '#f4511e',
        '#5e35b1', '#039be5', '#d81b60',
    ];

    window.awLeafletMap = {
        render(elementId, markers) {
            if (!window.L) {
                console.warn('Leaflet not loaded yet');
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
