window.DetectionExtensions = (function () {

    let video = null;
    let canvas = null;
    let ctx = null;
    let dotNet = null;
    let running = false;

    function startColorDetectionLoop(dotNetHelper) {
        console.log("✅ startColorDetectionLoop called");

        dotNet = dotNetHelper;
        video = document.getElementById("videoElement");
        canvas = document.getElementById("overlayCanvas");
        ctx = canvas.getContext("2d");
        running = true;

        requestAnimationFrame(loop);
    }

    function stop() {
        running = false;
    }

    function loop() {
        if (!running) return;
        if (!video || video.videoWidth === 0) {
            requestAnimationFrame(loop);
            return;
        }

        // ✅ Clear overlay
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // --- Read frame with OpenCV ---
        let src = new cv.Mat(video.videoHeight, video.videoWidth, cv.CV_8UC4);
        let cap = new cv.VideoCapture(video);
        cap.read(src);

        let hsv = new cv.Mat();
        cv.cvtColor(src, hsv, cv.COLOR_RGBA2RGB);
        cv.cvtColor(hsv, hsv, cv.COLOR_RGB2HSV);

        let gray = new cv.Mat();
        cv.cvtColor(src, gray, cv.COLOR_RGBA2GRAY);

        let surface = analyzeSurface(gray);

        // ✅ Visual overlay so you know it works
        ctx.font = "14px Arial";
        ctx.fillStyle = "yellow";
        ctx.fillText(
            `Brightness: ${surface.brightness.toFixed(1)}  Contrast: ${surface.contrast.toFixed(1)} (${surface.status})`,
            10,
            20
        );

        // ✅ Console debug
        console.log("🌞 Brightness:", surface.brightness, "⚡ Contrast:", surface.contrast, "Status:", surface.status);

        // ✅ Optional send to Blazor
        if (dotNet) {
            try {
                const payload = JSON.stringify(surface);
                console.log("➡️ Invoking .NET UpdateBrightnessContrast with payload:", payload);
                dotNet.invokeMethodAsync("UpdateBrightnessContrast", payload)
                    .catch(e => console.warn("JS->.NET invoke failed:", e));
            } catch (err) {
                console.warn("Failed to stringify surface payload:", err);
            }
        }

        let results = [];

        // ✅ Color ranges
        const colors = [
            {
                name: "Red",
                lower: [136, 87, 111],
                upper: [180, 255, 255],
                draw: "red"
            },
            {
                name: "Green",
                lower: [25, 52, 72],
                upper: [102, 255, 255],
                draw: "lime"
            },
            {
                name: "Blue",
                lower: [94, 150, 50],    // tighten S & V
                upper: [120, 255, 255],
                draw: "blue"
            },
            {
                name: "Yellow",
                lower: [20, 100, 100],
                upper: [30, 255, 255],
                draw: "yellow"
            },
            {
                name: "Black",
                lower: [0, 0, 0],
                upper: [180, 255, 30],
                draw: "black"
            },
            {
                name: "White",
                lower: [0, 0, 210],
                upper: [180, 15, 255],
                draw: "white"
            }

        ];


        let idCounter = 0;

        colors.forEach(c => {
            let low = new cv.Mat(hsv.rows, hsv.cols, hsv.type(),
                new cv.Scalar(c.lower[0], c.lower[1], c.lower[2], 255)
            );

            let high = new cv.Mat(hsv.rows, hsv.cols, hsv.type(),
                new cv.Scalar(c.upper[0], c.upper[1], c.upper[2], 255)
            );
            
            let mask = new cv.Mat();

            cv.inRange(hsv, low, high, mask);

            let contours = new cv.MatVector();
            let hierarchy = new cv.Mat();
            cv.findContours(mask, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);

            for (let i = 0; i < contours.size(); i++) {
                let cnt = contours.get(i);
                let area = cv.contourArea(cnt);
                if (area < 300) continue;

                let rect = cv.boundingRect(cnt);

                // ✅ Draw rectangle
                ctx.strokeStyle = c.draw;
                ctx.lineWidth = 2;
                ctx.strokeRect(rect.x, rect.y, rect.width, rect.height);
                ctx.fillStyle = c.draw;
                ctx.font = "12px Arial";
                ctx.fillText(c.name, rect.x, rect.y - 5);

                // ✅ Sample center pixel RGB
                let cx = rect.x + rect.width / 2;
                let cy = rect.y + rect.height / 2;

                let pixel = src.ucharPtr(cy, cx);
                let r = pixel[0];
                let g = pixel[1];
                let b = pixel[2];

                results.push({
                    id: idCounter++,
                    shape: "rect",
                    centerX: cx,
                    centerY: cy,
                    color: c.name,
                    rgb: { r, g, b }
                });
            }

            low.delete(); high.delete(); mask.delete(); contours.delete(); hierarchy.delete();
        });

        // ✅ Send to Blazor
        if (dotNet) {
            dotNet.invokeMethodAsync("UpdateDetectedColors", results);
        }

        src.delete();
        hsv.delete();

        requestAnimationFrame(loop);
        gray.delete();
    }

    function drawColorOverlay(canvasId, items) {
        const cvs = document.getElementById(canvasId);
        if (!cvs) return;
        const c = cvs.getContext("2d");

        c.clearRect(0, 0, cvs.width, cvs.height);

        items.forEach(i => {
            c.fillStyle = "yellow";
            c.fillText(`${i.color}`, i.centerX, i.centerY);
        });
    }

        function analyzeBrightness(grayMat) {
        return cv.mean(grayMat)[0];
    }

    function analyzeContrast(grayMat) {
        const meanVal = cv.mean(grayMat)[0];

        let meanMat = new cv.Mat(
            grayMat.rows,
            grayMat.cols,
            grayMat.type(),
            new cv.Scalar(meanVal, meanVal, meanVal, 255)
        );

        let diff = new cv.Mat();
        cv.absdiff(grayMat, meanMat, diff);

        const contrast = cv.mean(diff)[0];

        meanMat.delete();
        diff.delete();

        return contrast;
    }

    function analyzeSurface(grayMat) {
        const brightness = analyzeBrightness(grayMat);
        const contrast = analyzeContrast(grayMat);

        let status =
            brightness < 50 ? "too_dark" :
            brightness > 200 ? "too_bright" :
            contrast < 10 ? "low_contrast" :
            "ok";

        return {
            brightness,
            contrast,
            status
        };
    }

    // --- global scope ---
    window.OCRWorker = {
        worker: null,
        isReady: false
    };

    // --- crop selection state ---
    window.OCRCrop = {
        canvas: null,
        ctx: null,
        rect: null,        // user-drawn rect (canvas coords)
        lockedRect: null,  // locked area for OCR
        dragging: false,
        startX: 0,
        startY: 0
    };

    // ---------- Overlay helper functions (GLOBAL) ----------
    window.clearOverlay = function () {
        const crop = window.OCRCrop;
        if (!crop || !crop.ctx) return;
        crop.ctx.clearRect(0, 0, crop.canvas.width, crop.canvas.height);
    };

    window.drawCropOutline = function (r, color = "white", dashed = true) {
        const crop = window.OCRCrop;
        if (!crop || !crop.ctx || !r) return;

        const ctx = crop.ctx;
        ctx.clearRect(0, 0, crop.canvas.width, crop.canvas.height);

        ctx.beginPath();
        ctx.lineWidth = 2;
        ctx.strokeStyle = color;
        ctx.setLineDash(dashed ? [6, 4] : []);
        ctx.strokeRect(r.x, r.y, r.width, r.height);
        ctx.setLineDash([]);
    };

    window.drawCropRect = function (r) {
        // ✅ white dashed while dragging
        window.drawCropOutline(r, true);
    };

    window.drawLockedRect = function (r) {
        // ✅ solid white when locked
        window.drawCropOutline(r, false);
    };

    window.OCRCrop.renderLoop = function() {
        const crop = window.OCRCrop;
        if (!crop || !crop.ctx) return;

        // Clear canvas each frame
        crop.ctx.clearRect(0, 0, crop.canvas.width, crop.canvas.height);

        // Draw current rectangle if exists
        if (crop.rect) {
            crop.ctx.beginPath();
            crop.ctx.lineWidth = 2;
            // White dashed while dragging, orange solid after release
            crop.ctx.strokeStyle = crop.dragging ? "white" : "orange";
            crop.ctx.setLineDash(crop.dragging ? [6, 4] : []);
            crop.ctx.strokeRect(crop.rect.x, crop.rect.y, crop.rect.width, crop.rect.height);
            crop.ctx.setLineDash([]);
        }

        // Request next frame
        requestAnimationFrame(window.OCRCrop.renderLoop);
    };

    // ---------- Enable custom crop (attach listeners) ----------
    window.enableCustomCrop = function (overlayIdOrElement) {
    const canvas = typeof overlayIdOrElement === "string"
        ? document.getElementById(overlayIdOrElement)
        : overlayIdOrElement;

    if (!canvas) return console.warn("enableCustomCrop: overlay not found");
    if (canvas._customCropAttached) return;
    canvas._customCropAttached = true;

    const crop = window.OCRCrop;
    crop.canvas = canvas;
    crop.ctx = canvas.getContext("2d");
    crop.rect = null;
    crop.lockedRect = null;

    function getCanvasPos(e) {
        const rect = canvas.getBoundingClientRect();
        const clientX = e.touches ? e.touches[0].clientX : e.clientX;
        const clientY = e.touches ? e.touches[0].clientY : e.clientY;
        return {
        x: Math.round((clientX - rect.left) * (canvas.width / rect.width)),
        y: Math.round((clientY - rect.top) * (canvas.height / rect.height))
        };
    }

    function onDown(e) {
        e.preventDefault();
        const p = getCanvasPos(e);
        crop.startX = p.x;
        crop.startY = p.y;
        crop.rect = { x: p.x, y: p.y, width: 0, height: 0 };
        crop.dragging = true;
    }

    function onMove(e) {
        if (!crop.dragging) return;
        e.preventDefault();
        const p = getCanvasPos(e);
        crop.rect.x = Math.min(crop.startX, p.x);
        crop.rect.y = Math.min(crop.startY, p.y);
        crop.rect.width = Math.abs(p.x - crop.startX);
        crop.rect.height = Math.abs(p.y - crop.startY);
    }

    function onUp(e) {
        if (!crop.dragging) return;
        e.preventDefault();
        crop.dragging = false;
    }

    // mouse
    canvas.addEventListener("mousedown", onDown);
    canvas.addEventListener("mousemove", onMove);
    canvas.addEventListener("mouseup", onUp);
    // touch
    canvas.addEventListener("touchstart", onDown);
    canvas.addEventListener("touchmove", onMove);
    canvas.addEventListener("touchend", onUp);

    console.log("🟢 Custom crop enabled.");

    // start live render loop once
    if (!window.OCRCrop._loopStarted) {
        window.OCRCrop._loopStarted = true;
        window.OCRCrop.renderLoop();
    }

    };

    // Lock current crop (button)
    window.lockCropArea = function () {
    const crop = window.OCRCrop;
    if (!crop || !crop.rect) {
        alert("Please draw the crop area first.");
        return;
    }
    crop.lockedRect = { ...crop.rect };
    console.log("🔒 Crop locked:", crop.lockedRect);
    window.drawLockedRect(crop.lockedRect);
    };

    // ---------- OCR worker logic ----------
    async function initOCRWorker() {
    if (window.OCRWorker.isReady) return;

    const { createWorker } = Tesseract;

    window.OCRWorker.worker = await createWorker({
        logger: m => console.log("Tesseract:", m)
    });

    if (typeof window.OCRWorker.worker.load === "function") {
        await window.OCRWorker.worker.load();
    }
    await window.OCRWorker.worker.loadLanguage("eng");
    await window.OCRWorker.worker.initialize("eng");

    window.OCRWorker.isReady = true;
    console.log("✅ OCR Worker ready");
    }

    window.startOCR = async function (dotNetHelper) {
    const video = document.getElementById("videoElement");
    if (!video || video.videoWidth === 0) return;

    await initOCRWorker();

    const fullCanvas = document.createElement("canvas");
    fullCanvas.width = video.videoWidth;
    fullCanvas.height = video.videoHeight;
    const ctx = fullCanvas.getContext("2d");
    ctx.drawImage(video, 0, 0, fullCanvas.width, fullCanvas.height);

    const crop = window.OCRCrop.lockedRect;
    if (!crop) {
        alert("Please draw and lock the crop area first.");
        return;
    }

    // scale crop to video resolution
    const scaleX = video.videoWidth / window.OCRCrop.canvas.width;
    const scaleY = video.videoHeight / window.OCRCrop.canvas.height;

    const sx = Math.round(crop.x * scaleX);
    const sy = Math.round(crop.y * scaleY);
    const sw = Math.max(1, Math.round(crop.width * scaleX));
    const sh = Math.max(1, Math.round(crop.height * scaleY));

    const croppedCanvas = document.createElement("canvas");
    croppedCanvas.width = sw;
    croppedCanvas.height = sh;
    const croppedCtx = croppedCanvas.getContext("2d");

    croppedCtx.drawImage(
        fullCanvas,
        sx, sy, sw, sh,
        0, 0, sw, sh
    );

    const { data } = await window.OCRWorker.worker.recognize(croppedCanvas);

    if (dotNetHelper)
        dotNetHelper.invokeMethodAsync("UpdateOCRResult", data.text);

    console.log("📝 OCR Result:", data.text);
    };

    return {
        startColorDetectionLoop,
        stop,
        drawColorOverlay,

        analyzeBrightness,
        analyzeContrast,
        analyzeSurface,

        lockCropArea,
        startOCR
    };
})();
