// HTML5 video playback for the Jellyfin Avalonia browser client.
// Restored from npm by the project build, so the app loads no scripts from external hosts.
const HLS_URL = './lib/hls.js/hls.min.mjs';
const JASSUB_URL = './lib/jassub/jassub.js';
const LIBBITSUB_URL = './lib/libbitsub/libbitsub.js';

let Hls = null;
let hls = null;
let disposeSubtitleRenderer = null;
let subtitleId = 0;
let objectFit = 'contain';
let lastError = '';
let buffering = false;
let loadId = 0;

function video() {
    return document.getElementById('jellyfin-video');
}

function subtitles() {
    return document.getElementById('jellyfin-subtitles');
}

async function loadHls() {
    if (Hls === null) {
        const module = await import(HLS_URL);
        Hls = module.default;
    }
    return Hls;
}

function attachEvents(element) {
    if (element.dataset.attached) {
        return;
    }
    element.dataset.attached = 'true';
    element.addEventListener('resize', fitSubtitles);
    new ResizeObserver(fitSubtitles).observe(element);
    element.addEventListener('waiting', () => { buffering = true; });
    element.addEventListener('playing', () => { buffering = false; });
    element.addEventListener('canplay', () => { buffering = false; });
    element.addEventListener('error', () => {
        const error = element.error;
        lastError = error ? `Media error ${error.code}: ${error.message || ''}` : 'Media error';
    });
}

export function canPlayType(type) {
    const element = video() || document.createElement('video');
    return element.canPlayType(type) || '';
}

export function isTypeSupported(type) {
    try {
        return !!(globalThis.MediaSource && globalThis.MediaSource.isTypeSupported(type));
    } catch {
        return false;
    }
}

export function supportsNativeHls() {
    return canPlayType('application/vnd.apple.mpegurl') !== '';
}

// Chromium 141+ also reports native HLS, but its player can't switch audio tracks or HDR/SDR playlists, so only
// WebKit plays HLS natively. Chromium is the only engine that exposes navigator.userAgentData.
function preferNativeHls() {
    return supportsNativeHls() && !navigator.userAgentData;
}

// jassub renders on an OffscreenCanvas from a module worker.
export function supportsAssRenderer() {
    return typeof OffscreenCanvas !== 'undefined'
        && typeof WebAssembly === 'object'
        && 'transferControlToOffscreen' in HTMLCanvasElement.prototype;
}

// The whole page goes full screen, since the video element sits behind the Avalonia canvas.
export function supportsFullscreen() {
    return document.fullscreenEnabled === true;
}

export function isFullscreen() {
    return document.fullscreenElement != null;
}

export function setFullscreen(fullscreen) {
    if (fullscreen === isFullscreen()) {
        return;
    }

    const request = fullscreen ? document.documentElement.requestFullscreen() : document.exitFullscreen();
    request.catch((error) => console.warn('Failed to change full screen', error));
}

export function getScreenHeight() {
    return Math.round((globalThis.screen?.height || 1080) * (globalThis.devicePixelRatio || 1));
}

// A play() interrupted by pause() or a newer load() rejects with AbortError, which is not a playback error.
function reportPlayError(id, error) {
    if (id === loadId && error?.name !== 'AbortError') {
        lastError = String(error);
    }
}

export async function load(url, isHls, startSeconds) {
    const id = ++loadId;
    const element = video();
    attachEvents(element);
    stop();
    lastError = '';
    buffering = true;
    element.classList.add('visible');

    if (isHls && !preferNativeHls()) {
        const HlsClass = await loadHls();
        if (id !== loadId) {
            return;
        }

        if (HlsClass.isSupported()) {
            hls = new HlsClass({ startPosition: startSeconds > 0 ? startSeconds : -1 });
            hls.on(HlsClass.Events.ERROR, (_, data) => {
                if (data.fatal) {
                    lastError = `HLS error: ${data.details}`;
                }
            });
            hls.loadSource(url);
            hls.attachMedia(element);
            await element.play().catch(() => {});
            return;
        }
    }

    element.src = url;
    if (startSeconds > 0) {
        element.addEventListener('loadedmetadata', () => {
            if (id === loadId) {
                element.currentTime = startSeconds;
            }
        }, { once: true });
    }
    await element.play().catch((e) => reportPlayError(id, e));
}

export function play() {
    const id = loadId;
    video().play().catch((e) => reportPlayError(id, e));
}

export function pause() {
    video().pause();
}

export function stop() {
    const element = video();
    if (hls) {
        hls.destroy();
        hls = null;
    }
    clearSubtitles();
    element.pause();
    element.removeAttribute('src');
    element.load();
    element.classList.remove('visible');
}

export function seek(seconds) {
    video().currentTime = seconds;
}

export function getState() {
    const element = video();
    // position|duration|paused|ended|buffering|error
    const duration = Number.isFinite(element.duration) ? element.duration : 0;
    return [
        element.currentTime || 0,
        duration,
        element.paused ? 1 : 0,
        element.ended ? 1 : 0,
        buffering ? 1 : 0,
    ].join('|') + '|' + lastError;
}

export function setVolume(volume) {
    video().volume = Math.min(1, Math.max(0, volume / 100));
}

export function setMuted(muted) {
    video().muted = muted;
}

export function setRate(rate) {
    video().playbackRate = rate;
}

export function setObjectFit(fit) {
    objectFit = fit;
    video().style.objectFit = fit;
    fitSubtitles();
}

// jassub and libbitsub size their canvas to the video's contain box, centred in the element. Scale the subtitles from
// the centre onto the box that cover or fill draws the video in.
function fitSubtitles() {
    const { clientWidth: width, clientHeight: height, videoWidth, videoHeight } = video();
    let scaleX = 1;
    let scaleY = 1;
    if (width > 0 && height > 0 && videoWidth > 0 && videoHeight > 0) {
        const contain = Math.min(width / videoWidth, height / videoHeight);
        if (objectFit === 'cover') {
            scaleX = scaleY = Math.max(width / videoWidth, height / videoHeight) / contain;
        } else if (objectFit === 'fill') {
            scaleX = width / (videoWidth * contain);
            scaleY = height / (videoHeight * contain);
        }
    }

    subtitles().style.transform = scaleX === 1 && scaleY === 1 ? '' : `scale(${scaleX}, ${scaleY})`;
}

function stopSubtitleRenderer() {
    subtitleId++;
    disposeSubtitleRenderer?.();
    disposeSubtitleRenderer = null;
}

function clearSubtitles() {
    stopSubtitleRenderer();
    for (const track of Array.from(video().querySelectorAll('track'))) {
        track.remove();
    }
}

export async function loadSubtitle(url, format, language, fonts) {
    clearSubtitles();
    const id = subtitleId;
    try {
        switch (format.toLowerCase()) {
            case 'ass':
            case 'ssa':
                await renderAss(id, url, fonts);
                break;
            case 'pgs':
            case 'pgssub':
            case 'sup':
                await renderPgs(id, url);
                break;
            default:
                addTextTrack(url, language);
                break;
        }
    } catch (error) {
        console.error('Failed to load subtitles', error);
    }
}

async function renderAss(id, url, fonts) {
    const { default: JASSUB } = await import(JASSUB_URL);
    if (id !== subtitleId) {
        return;
    }

    const canvas = document.createElement('canvas');
    canvas.style.position = 'absolute';
    subtitles().appendChild(canvas);

    // The worker, wasm and default font resolve next to the bundled module.
    const renderer = new JASSUB({ video: video(), canvas, subUrl: url, fonts });
    disposeSubtitleRenderer = () => renderer.destroy().catch(() => {});
}

async function renderPgs(id, url) {
    const { PgsRenderer } = await import(LIBBITSUB_URL);
    if (id !== subtitleId) {
        return;
    }

    const renderer = new PgsRenderer({
        video: video(),
        container: subtitles(),
        subUrl: url,
        onError: (error) => console.error('Failed to render PGS subtitles', error),
    });
    disposeSubtitleRenderer = () => renderer.dispose();
}

function addTextTrack(url, language) {
    const element = video();
    const track = document.createElement('track');
    track.kind = 'subtitles';
    track.src = url;
    track.srclang = language || 'und';
    track.default = true;
    element.appendChild(track);
    track.addEventListener('load', () => { track.track.mode = 'showing'; });
    track.track.mode = 'showing';
}

export function selectTextTrack(index) {
    stopSubtitleRenderer();
    const tracks = video().textTracks;
    for (let i = 0; i < tracks.length; i++) {
        tracks[i].mode = i === index ? 'showing' : 'disabled';
    }
    if (hls && index >= 0) {
        hls.subtitleTrack = index;
    }
}

export function selectAudioTrack(index) {
    if (hls) {
        hls.audioTrack = index;
        return;
    }
    const tracks = video().audioTracks;
    if (tracks) {
        for (let i = 0; i < tracks.length; i++) {
            tracks[i].enabled = i === index;
        }
    }
}
