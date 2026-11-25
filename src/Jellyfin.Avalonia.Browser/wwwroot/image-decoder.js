// WebAssembly runs the app on the main thread, so decoding in Skia there stalls scrolling. Workers decode instead.

const WORKER_URL = new URL('./image-decoder-worker.js', import.meta.url);
const WORKER_COUNT = Math.min(4, Math.max(1, Math.floor((navigator.hardwareConcurrency || 2) / 2)));

const workers = [];
const pending = new Map();
let nextId = 0;

export function isSupported() {
    return typeof Worker === 'function'
        && typeof OffscreenCanvas === 'function'
        && typeof createImageBitmap === 'function';
}

function onMessage({ data }) {
    const request = pending.get(data.id);
    if (!request) {
        return;
    }

    pending.delete(data.id);
    request.worker.busy--;
    if (data.error) {
        request.reject(new Error(data.error));
    } else {
        request.resolve(data);
    }
}

function onError(worker, event) {
    // A worker that fails to load or crashes fails its requests, and the app decodes those itself.
    event.preventDefault?.();
    for (const [id, request] of pending) {
        if (request.worker === worker) {
            pending.delete(id);
            request.reject(new Error(event.message || 'Image decoder worker failed'));
        }
    }

    workers.splice(workers.indexOf(worker), 1);
    worker.instance.terminate();
}

function getWorker() {
    let worker = workers.reduce((least, w) => (least === null || w.busy < least.busy ? w : least), null);
    if (worker === null || (worker.busy > 0 && workers.length < WORKER_COUNT)) {
        worker = { instance: new Worker(WORKER_URL), busy: 0 };
        worker.instance.onmessage = onMessage;
        worker.instance.onerror = event => onError(worker, event);
        workers.push(worker);
    }

    return worker;
}

// The view wraps a pinned .NET buffer, so it is copied before this returns.
export function decode(view, width, blur) {
    const bytes = view.slice();
    view.dispose();

    const id = nextId++;
    const worker = getWorker();
    worker.busy++;
    return new Promise((resolve, reject) => {
        pending.set(id, { worker, resolve, reject });
        worker.instance.postMessage({ id, bytes, width, blur }, [bytes.buffer]);
    });
}

export function copyPixels(image, view) {
    // MemoryView.set only takes the array type of the view, and getImageData returns a Uint8ClampedArray.
    const pixels = image.pixels;
    view.set(new Uint8Array(pixels.buffer, pixels.byteOffset, pixels.length));
    image.pixels = null;
}
