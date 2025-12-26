window.CircleDetection = {
    circles: [],                // Detected or dragged circles
    draggedCircles: [],         // For drag mode
    isDraggingCircle: false,    
    centroid: null,             // Center of all circles

    // -------------------------
    // Detect circles from source
    // -------------------------
    detectCirclesFromSource: function (sourceId, processingCanvasId) {
        console.log("detectCirclesFromSource called:", sourceId, processingCanvasId);
        window.prepareCanvasForDrawing('previewImage', 'previewCanvas');

        const source = document.getElementById(sourceId);
        const processingCanvas = document.getElementById(processingCanvasId);
        const overlayCanvas = document.getElementById("previewCanvas");

        if (!source) return console.error("Source element not found:", sourceId);

        const pCtx = processingCanvas.getContext("2d");
        const oCtx = overlayCanvas.getContext("2d");

        // Resize processing canvas to source resolution
        if (source.tagName === "VIDEO") {
            if (source.readyState < 2) { setTimeout(() => this.detectCirclesFromSource(sourceId, processingCanvasId), 100); return; }
            processingCanvas.width = source.videoWidth;
            processingCanvas.height = source.videoHeight;
            pCtx.drawImage(source, 0, 0);
        } else if (source.tagName === "IMG") {
            if (!source.complete) { source.onload = () => this.detectCirclesFromSource(sourceId, processingCanvasId); return; }
            processingCanvas.width = source.naturalWidth;
            processingCanvas.height = source.naturalHeight;
            pCtx.drawImage(source, 0, 0);
        } else if (source.tagName === "CANVAS") {
            processingCanvas.width = source.width;
            processingCanvas.height = source.height;
            pCtx.drawImage(source, 0, 0);
        }

        // OpenCV circle detection
        let src = cv.imread(processingCanvas);
        let gray = new cv.Mat();
        cv.cvtColor(src, gray, cv.COLOR_RGBA2GRAY);
        cv.medianBlur(gray, gray, 5);

        let circlesMat = new cv.Mat();
        cv.HoughCircles(gray, circlesMat, cv.HOUGH_GRADIENT, 1, 50, 100, 30, 0, 0);

        oCtx.clearRect(0, 0, overlayCanvas.width, overlayCanvas.height);
        oCtx.drawImage(source, 0, 0, overlayCanvas.width, overlayCanvas.height);

        const scaleX = overlayCanvas.width / processingCanvas.width;
        const scaleY = overlayCanvas.height / processingCanvas.height;

        this.circles = [];
        let sumX = 0, sumY = 0;

        for (let i = 0; i < circlesMat.cols; i++) {
            const x = circlesMat.data32F[i * 3];
            const y = circlesMat.data32F[i * 3 + 1];
            const r = circlesMat.data32F[i * 3 + 2];

            this.circles.push({ x, y, r });
            sumX += x; sumY += y;

            const dx = x * scaleX, dy = y * scaleY, dr = r * scaleX;

            // Circle outline
            oCtx.beginPath();
            oCtx.arc(dx, dy, dr, 0, 2 * Math.PI);
            oCtx.lineWidth = 2;
            oCtx.strokeStyle = "black";
            oCtx.stroke();

            // Center point
            oCtx.beginPath();
            oCtx.arc(dx, dy, 4, 0, 2 * Math.PI);
            oCtx.fillStyle = "red";
            oCtx.fill();
        }

        if (this.circles.length > 0) {
            const centerX = sumX / this.circles.length;
            const centerY = sumY / this.circles.length;
            this.centroid = { x: centerX, y: centerY };

            const cdx = centerX * scaleX, cdy = centerY * scaleY;
            oCtx.beginPath();
            oCtx.arc(cdx, cdy, 6, 0, 2 * Math.PI);
            oCtx.fillStyle = "blue";
            oCtx.fill();

            oCtx.strokeStyle = "blue";
            oCtx.lineWidth = 2;
            oCtx.beginPath();
            oCtx.moveTo(cdx - 10, cdy); oCtx.lineTo(cdx + 10, cdy);
            oCtx.moveTo(cdx, cdy - 10); oCtx.lineTo(cdx, cdy + 10);
            oCtx.stroke();
        }

        console.log("Circles detected:", this.circles);
        src.delete(); gray.delete(); circlesMat.delete();
    },

    // -------------------------
    // Enable drag multi-circle mode
    // -------------------------
    enableDragCircleMode: function (overlayCanvasId) {
        const overlay = document.getElementById(overlayCanvasId);
        if (!overlay) return;

        if (overlay._dragCircleAttached) return;
        overlay._dragCircleAttached = true;

        const ctx = overlay.getContext("2d");
        let startX = 0, startY = 0, activeCircle = null;

        CircleDetection.draggedCircles = [];
        CircleDetection.isDraggingCircle = false;

        // --- Draw the image immediately on the canvas ---
        const img = document.getElementById('previewImage');
        if(img){
            ctx.clearRect(0,0,overlay.width, overlay.height);
            ctx.drawImage(img, 0, 0, overlay.width, overlay.height);
        }

        function getCanvasPos(e) {
            const r = overlay.getBoundingClientRect();
            const clientX = e.touches ? e.touches[0].clientX : e.clientX;
            const clientY = e.touches ? e.touches[0].clientY : e.clientY;
            return { x: (clientX - r.left) * (overlay.width / r.width), y: (clientY - r.top) * (overlay.height / r.height) };
        }

        function redrawAll() {
            // redraw image first
            if(img){
                ctx.clearRect(0,0,overlay.width, overlay.height);
                ctx.drawImage(img, 0, 0, overlay.width, overlay.height);
            } else {
                ctx.clearRect(0,0,overlay.width, overlay.height);
            }

            for (const c of CircleDetection.draggedCircles) {
                ctx.beginPath(); ctx.arc(c.x, c.y, c.r, 0, Math.PI*2);
                ctx.strokeStyle = "white"; ctx.lineWidth = 2; ctx.stroke();
                ctx.beginPath(); ctx.arc(c.x, c.y, 3, 0, Math.PI*2);
                ctx.fillStyle = "red"; ctx.fill();
            }

            if(activeCircle){
                ctx.setLineDash([6,4]);
                ctx.beginPath(); ctx.arc(activeCircle.x, activeCircle.y, activeCircle.r, 0, 2*Math.PI);
                ctx.stroke(); ctx.setLineDash([]);
            }
        }

        function onDown(e){
            e.preventDefault();
            const p = getCanvasPos(e);
            startX = p.x; startY = p.y;
            CircleDetection.isDraggingCircle = true;
            activeCircle = {x:startX, y:startY, r:1};
        }

        function onMove(e){
            if(!CircleDetection.isDraggingCircle || !activeCircle) return;
            e.preventDefault();
            const p = getCanvasPos(e);
            const dx = p.x - startX, dy = p.y - startY;
            activeCircle.r = Math.sqrt(dx*dx + dy*dy);
            redrawAll();
        }

        function onUp(e){
            if(!CircleDetection.isDraggingCircle || !activeCircle) return;
            e.preventDefault();
            CircleDetection.isDraggingCircle = false;
            CircleDetection.draggedCircles.push(activeCircle);
            activeCircle = null;
            redrawAll();
            CircleDetection.syncDraggedCirclesToDetection();
        }

        overlay.addEventListener("mousedown", onDown);
        overlay.addEventListener("mousemove", onMove);
        overlay.addEventListener("mouseup", onUp);
        overlay.addEventListener("touchstart", onDown);
        overlay.addEventListener("touchmove", onMove);
        overlay.addEventListener("touchend", onUp);

        // Disable function
        window._dragCircleMode = window._dragCircleMode || {};
        window._dragCircleMode.disable = () => {
            overlay._dragCircleAttached = false;
            const clone = overlay.cloneNode(true);
            overlay.parentNode.replaceChild(clone, overlay);
            CircleDetection.draggedCircles = [];
            console.log("🔴 Drag circle mode disabled");
        };

        console.log("🟢 Drag circle mode enabled");
    },

    // -------------------------
    // Sync dragged circles → circles array + centroid
    // -------------------------
    syncDraggedCirclesToDetection: function () {
        CircleDetection.circles = CircleDetection.draggedCircles.map(c => ({ x:c.x, y:c.y, r:c.r }));
        if (CircleDetection.circles.length === 0) return;
        let sumX=0, sumY=0;
        for (const c of CircleDetection.circles) { sumX += c.x; sumY += c.y; }
        CircleDetection.centroid = { x: sumX / CircleDetection.circles.length, y: sumY / CircleDetection.circles.length };
        console.log("🔵 Updated centroid from dragged circles:", CircleDetection.centroid);
    }
};

window.adjustCanvasToImage = function () {
    const img = document.getElementById("previewImage");
    const canvas = document.getElementById("previewCanvas");

    if (!img || !canvas) {
        console.log("Image or canvas not ready, retry...");
        setTimeout(window.adjustCanvasToImage, 100);
        return;
    }

    const rect = img.getBoundingClientRect();

    canvas.style.width = rect.width + "px";
    canvas.style.height = rect.height + "px";

    canvas.width = rect.width;
    canvas.height = rect.height;

    console.log("Canvas adjusted:", rect.width, rect.height);
};

window.prepareCanvasForDrawing = function(imgId, canvasId) {
    const img = document.getElementById(imgId);
    const canvas = document.getElementById(canvasId);
    if (!img || !canvas) return;

    const rect = img.getBoundingClientRect();

    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;

    // scale canvas to match displayed size
    canvas.style.width = rect.width + "px";
    canvas.style.height = rect.height + "px";

    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

    img.style.display = "none";
    canvas.style.display = "block";
};

window.drawImageToCanvasAndHideImg = function (imgId, canvasId) {
    const img = document.getElementById(imgId);
    const canvas = document.getElementById(canvasId);

    if (!img || !canvas) return;

    const rect = img.getBoundingClientRect();

    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;

    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

    // scale canvas to match displayed image size
    canvas.style.width = rect.width + "px";
    canvas.style.height = rect.height + "px";

    img.style.display = "none";
    canvas.style.display = "block";
};

window.restoreImageFromCanvas = function (imgId, canvasId) {
    const img = document.getElementById(imgId);
    const canvas = document.getElementById(canvasId);
    if (!img || !canvas) return;

    canvas.style.display = "none";
    img.style.display = "block";
};
