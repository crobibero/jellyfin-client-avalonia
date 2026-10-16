// jassub and libbitsub are bundled because workers ignore import maps, so their bare and extensionless imports can't resolve.
import { cpSync, readFileSync, rmSync } from 'node:fs';
import { join } from 'node:path';
import { build } from 'esbuild';

const outdir = process.argv[2];
if (!outdir) {
    throw new Error('Usage: node build-lib.mjs <outdir>');
}

rmSync(outdir, { recursive: true, force: true });

const copies = {
    'hls.js/hls.min.mjs': 'hls.js/dist/hls.min.mjs',
    'hls.js/LICENSE': 'hls.js/LICENSE',
    'jassub/wasm/jassub-worker.wasm': 'jassub/dist/wasm/jassub-worker.wasm',
    'jassub/wasm/jassub-worker-modern.wasm': 'jassub/dist/wasm/jassub-worker-modern.wasm',
    'jassub/default.woff2': 'jassub/dist/default.woff2',
    'jassub/LICENSE': 'jassub/LICENSE',
    'libbitsub/pkg/libbitsub.js': 'libbitsub/pkg/libbitsub.js',
    'libbitsub/pkg/libbitsub_bg.wasm': 'libbitsub/pkg/libbitsub_bg.wasm',
    'libbitsub/LICENSE': 'libbitsub/LICENSE',
};
for (const [target, source] of Object.entries(copies)) {
    cpSync(join('node_modules', source), join(outdir, target));
}

// libbitsub finds its glue and wasm at ../../pkg from dist/ts/wasm.js. Point it at the copy served next to the bundle,
// and load the glue by URL, so the main thread and the worker share the one served file.
const libbitsubPaths = {
    name: 'libbitsub-paths',
    setup(build) {
        build.onLoad({ filter: /libbitsub[\\/]dist[\\/]ts[\\/]wasm\.js$/ }, ({ path }) => {
            const source = readFileSync(path, 'utf8');
            const contents = source
                .replace("import('../../pkg/libbitsub')", 'import(getWasmGlueUrl())')
                .replaceAll("'../../pkg/", "'./pkg/");
            if (contents.includes('../../pkg/') || !contents.includes('import(getWasmGlueUrl())')) {
                throw new Error('libbitsub wasm.js changed; update the path rewrite');
            }
            return { contents, loader: 'js' };
        });
    },
};

await build({
    // jassub resolves its wasm and default font against import.meta.url, so the entries keep its dist layout.
    entryPoints: {
        'jassub/jassub': 'node_modules/jassub/dist/jassub.js',
        'jassub/worker/worker': 'node_modules/jassub/dist/worker/worker.js',
        'libbitsub/libbitsub': 'node_modules/libbitsub/dist/index.js',
    },
    plugins: [libbitsubPaths],
    outdir,
    bundle: true,
    format: 'esm',
    minify: true,
    legalComments: 'inline',
    target: 'es2022',
    logLevel: 'warning',
});
