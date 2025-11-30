const CACHE_NAME = 'tu-app-v1.0';
const urlsToCache = [
    '/',
    '/css/site.css',
    '/site.js',
    'js/actividad.js',
    'js/cultivo.js',
    'js/actividadRapida.js',
    'js/administradorLicencias.js',
    'js/campania.js',
    'js/campo.js',
    'js/Gasto.js',
    'js/registroClima.js',
    'js/selectorCampania.js',
    'js/usuario.js'
];

self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME)
            .then(cache => cache.addAll(urlsToCache))
    );
});

self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request)
            .then(response => {
                // Devuelve el recurso en caché o lo busca en red
                return response || fetch(event.request);
            })
    );
});