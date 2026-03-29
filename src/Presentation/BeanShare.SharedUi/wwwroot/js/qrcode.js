// QR code download and print utilities for BeanShare coffee stations
window.beanshareQr = {

    downloadStationQr: function (base64DataUri, productName, spaceName) {
        var safeProduct = (productName || 'coffee').replace(/[^a-zA-Z0-9-_ ]/g, '').replace(/\s+/g, '-');
        var safeSpace = (spaceName || 'BeanShare').replace(/[^a-zA-Z0-9-_ ]/g, '').replace(/\s+/g, '-');
        var fileName = 'BeanShare-QR-' + safeSpace + '-' + safeProduct + '.png';
        this._triggerBlobDownload(base64DataUri, fileName);
    },

    // Kept for backward compatibility with existing Blazor interop calls
    downloadPng: function (base64DataUri, fileName) {
        this._triggerBlobDownload(base64DataUri, fileName || 'BeanShare-QR.png');
    },

    printQr: function (base64DataUri, productLabel, spaceName) {
        var printTarget = window.open('', '_blank', 'width=420,height=560');
        if (!printTarget) {
            return;
        }

        var markup = this._buildSinglePrintPage(base64DataUri, productLabel, spaceName);
        printTarget.document.write(markup);
        printTarget.document.close();

        this._printWhenReady(printTarget);
    },

    printAllQr: function (qrCodes, spaceName) {
        var printTarget = window.open('', '_blank', 'width=800,height=600');
        if (!printTarget) {
            return;
        }

        var markup = this._buildBatchPrintPage(qrCodes, spaceName);
        printTarget.document.write(markup);
        printTarget.document.close();

        this._printWhenReady(printTarget);
    },

    // --- Internal helpers ---

    _triggerBlobDownload: function (dataUri, fileName) {
        try {
            var blob = this._decodeDataUri(dataUri);
            var blobUrl = URL.createObjectURL(blob);

            var anchor = document.createElement('a');
            anchor.href = blobUrl;
            anchor.download = fileName;
            anchor.style.display = 'none';
            anchor.setAttribute('data-enhance-nav', 'false');
            document.body.appendChild(anchor);
            anchor.click();

            setTimeout(function () {
                document.body.removeChild(anchor);
                URL.revokeObjectURL(blobUrl);
            }, 3000);
        } catch (err) {
            console.error('[BeanShare] QR download failed:', err.message || err);
            window.open(dataUri, '_blank');
        }
    },

    _decodeDataUri: function (dataUri) {
        var parts = dataUri.split(',');
        var mimeMatch = parts[0].match(/:(.*?);/);
        var mimeType = mimeMatch ? mimeMatch[1] : 'image/png';
        var raw = atob(parts[1]);
        var buffer = new Uint8Array(raw.length);
        for (var i = 0; i < raw.length; i++) {
            buffer[i] = raw.charCodeAt(i);
        }
        return new Blob([buffer], { type: mimeType });
    },

    _printWhenReady: function (targetWindow) {
        var images = targetWindow.document.querySelectorAll('img');
        var remaining = images.length;

        var triggerPrint = function () {
            targetWindow.focus();
            targetWindow.print();
        };

        if (remaining === 0) {
            triggerPrint();
            return;
        }

        var onImageReady = function () {
            remaining--;
            if (remaining <= 0) {
                triggerPrint();
            }
        };

        for (var i = 0; i < images.length; i++) {
            if (images[i].complete && images[i].naturalWidth > 0) {
                onImageReady();
            } else {
                images[i].onload = onImageReady;
            }
        }

        // Safety net — some browsers don't fire onload for inline base64 images
        setTimeout(triggerPrint, 800);
    },

    _printStyles: function () {
        return 'body { margin: 0; padding: 1.5rem; font-family: sans-serif; }' +
            'img { display: block; margin: 0 auto; }' +
            '.label { font-size: 1.1rem; font-weight: 700; margin: 0.5rem 0 0.25rem; text-align: center; }' +
            '.sub { font-size: 0.8rem; color: #555; text-align: center; margin: 0; }' +
            '.hint { font-size: 0.7rem; color: #999; text-align: center; margin-top: 0.75rem; }' +
            '.footer { font-size: 0.6rem; color: #bbb; text-align: center; margin-top: 0.5rem; border-top: 1px solid #eee; padding-top: 0.5rem; }' +
            '.grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1.25rem; }' +
            '.card { text-align: center; border: 1px solid #ddd; border-radius: 0.5rem; padding: 0.75rem; break-inside: avoid; }' +
            '.card img { max-width: 160px; margin: 0 auto 0.375rem; }' +
            '.title { font-size: 1.1rem; font-weight: 600; margin: 0 0 0.75rem; }' +
            '@media print { body { padding: 0.75rem; } }';
    },

    _buildSinglePrintPage: function (imageData, label, spaceName) {
        return '<!DOCTYPE html><html><head><title>BeanShare QR - ' + (label || 'Coffee Station') + '</title>' +
            '<style>' + this._printStyles() + '</style></head><body>' +
            '<img src="' + imageData + '" alt="BeanShare QR" style="max-width: 280px;" />' +
            '<p class="label">' + (label || '') + '</p>' +
            (spaceName ? '<p class="sub">' + spaceName + '</p>' : '') +
            '<p class="hint">Scan to log your coffee consumption</p>' +
            '<p class="footer">BeanShare &mdash; Coffee Consumption Tracking</p>' +
            '</body></html>';
    },

    _buildBatchPrintPage: function (qrCodes, spaceName) {
        var html = '<!DOCTYPE html><html><head><title>BeanShare QR Codes - ' + (spaceName || '') + '</title>' +
            '<style>' + this._printStyles() + '</style></head><body>' +
            '<p class="title">' + (spaceName || 'BeanShare') + ' &mdash; Coffee Station QR Codes</p>' +
            '<div class="grid">';

        for (var i = 0; i < qrCodes.length; i++) {
            var qr = qrCodes[i];
            html += '<div class="card">' +
                '<img src="' + qr.base64 + '" alt="QR" />' +
                '<p class="label">' + (qr.label || '') + '</p>' +
                '<p class="sub">' + (qr.product || '') + '</p>' +
                '</div>';
        }

        html += '</div>' +
            '<p class="footer">BeanShare &mdash; Coffee Consumption Tracking</p>' +
            '</body></html>';
        return html;
    }
};
