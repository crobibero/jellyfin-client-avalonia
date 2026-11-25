// Decodes images off the main thread, which runs the app and renders it.
// Replies with premultiplied RGBA pixels, which the app copies straight into a bitmap.

self.onmessage = async ({ data: { id, bytes, width, blur } }) => {
    try {
        const source = await createImageBitmap(new Blob([bytes]));
        let targetWidth = source.width;
        let targetHeight = source.height;
        if (width > 0 && width !== targetWidth) {
            targetHeight = Math.max(1, Math.round(targetHeight * width / targetWidth));
            targetWidth = width;
        }

        // willReadFrequently keeps the canvas in CPU memory, so getImageData doesn't read back from the GPU.
        const canvas = new OffscreenCanvas(targetWidth, targetHeight);
        const context = canvas.getContext('2d', { willReadFrequently: true });
        context.imageSmoothingQuality = 'medium';
        if (blur > 0) {
            if (!('filter' in context)) {
                throw new Error('Canvas filters are not supported');
            }

            // The filter blurs transparency in past the edges of what is drawn. Drawing slightly past the canvas keeps
            // the edges opaque.
            const margin = Math.ceil(blur * 3);
            context.filter = `blur(${blur}px)`;
            context.drawImage(source, -margin, -margin, targetWidth + 2 * margin, targetHeight + 2 * margin);
        } else {
            context.drawImage(source, 0, 0, targetWidth, targetHeight);
        }

        source.close();

        const pixels = context.getImageData(0, 0, targetWidth, targetHeight).data;
        const opaque = premultiply(pixels);
        self.postMessage({ id, width: targetWidth, height: targetHeight, opaque, pixels }, [pixels.buffer]);
    } catch (error) {
        self.postMessage({ id, error: String(error) });
    }
};

// getImageData returns straight alpha; the clamped array rounds each product. Returns whether every pixel is opaque,
// so the app can skip blending.
function premultiply(pixels) {
    let opaque = true;
    for (let i = 3; i < pixels.length; i += 4) {
        const alpha = pixels[i];
        if (alpha === 255) {
            continue;
        }

        opaque = false;
        pixels[i - 3] = pixels[i - 3] * alpha / 255;
        pixels[i - 2] = pixels[i - 2] * alpha / 255;
        pixels[i - 1] = pixels[i - 1] * alpha / 255;
    }

    return opaque;
}
