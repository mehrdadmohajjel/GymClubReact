const CACHE_NAME = "gymmgr-v1";
const urlsToCache = ["/", "/index.html"];

self.addEventListener("install", (event: any) => {
    event.waitUntil(caches.open(CACHE_NAME).then(cache => cache.addAll(urlsToCache)));
    self.skipWaiting();
});

self.addEventListener("activate", (event: any) => {
    event.waitUntil(self.Clients.claim());
});

self.addEventListener("fetch", (event: any) => {
    event.respondWith(
        caches.match(event.request).then(response => response || fetch(event.request))
    );
});
