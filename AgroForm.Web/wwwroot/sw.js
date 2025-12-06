const CACHE_NAME = "gestionapp-v1";
const CORE_ASSETS = [
    "/",
    '/css/site.css',
    '/site.js',
    "/manifest.json",
    "/images/android-launchericon-192x192.png",
    "/images/android-launchericon-512x512.png",
    "/images/android-launchericon-180x180.png",
    "/images/android-launchericon-1024x1024.png",
    "/images/screenshot-1024x1536.png"
];

// INSTALACIÓN
self.addEventListener("install", (event) => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => cache.addAll(CORE_ASSETS))
    );
    self.skipWaiting();
});

// ACTIVACIÓN (limpiar versiones viejas)
self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(
                keys.filter(key => key !== CACHE_NAME)
                    .map(key => caches.delete(key))
            )
        )
    );
    self.clients.claim();
});

// FETCH: Cache first + network fallback + dynamic cache
self.addEventListener("fetch", (event) => {
    event.respondWith(
        caches.match(event.request).then(cachedResponse => {
            if (cachedResponse) return cachedResponse;

            return fetch(event.request).then(networkResponse => {
                return caches.open(CACHE_NAME).then(cache => {
                    // Cache dinámico (solo GET)
                    if (event.request.method === "GET") {
                        cache.put(event.request, networkResponse.clone());
                    }
                    return networkResponse;
                });
            });
        })
    );
});
