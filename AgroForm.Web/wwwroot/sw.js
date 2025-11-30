const CACHE_NAME = 'tu-app-v1.0';
const urlsToCache = [
    '/',
    '/css/site.css',
    '/site.js',
    'js/view/actividad.js',
    'js/view/cultivo.js',
    'js/view/actividadRapida.js',
    'js/view/administradorLicencias.js',
    'js/view/campania.js',
    'js/view/campo.js',
    'js/view/Gasto.js',
    'js/view/registroClima.js',
    'js/view/selectorCampania.js',
    'js/view/usuario.js'
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