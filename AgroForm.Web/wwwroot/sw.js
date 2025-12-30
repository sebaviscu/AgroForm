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
    self.skipWaiting();
});

// ACTIVACIÓN (limpiar versiones viejas)
self.addEventListener("activate", (event) => {
    self.clients.claim();
});
