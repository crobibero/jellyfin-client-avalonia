// HTML5 video playback for the Jellyfin Avalonia browser client.
const HLS_URL = 'https://cdn.jsdelivr.net/npm/hls.js@1.7.3/dist/hls.mjs';

let Hls = null;
let hls = null;
let lastError = '';
let buffering = false;
let loadId = 0;

function video() {
    return document.getElementById('jellyfin-video');
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

    if (isHls && !supportsNativeHls()) {
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
    for (const track of Array.from(element.querySelectorAll('track'))) {
        track.remove();
    }
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
    video().style.objectFit = fit;
}

export function addTextTrack(url, language) {
    const element = video();
    for (const track of Array.from(element.querySelectorAll('track'))) {
        track.remove();
    }
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
