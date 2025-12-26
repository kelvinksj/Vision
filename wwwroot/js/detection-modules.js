window.DetectionModules = (function () {
  let uniqueIdCounter = 0;       // 🆔 incremental unique ID
  let SelectionMode = "discover";       // "discover" | "focus"
  let latestDetections = [];            // store latest detections for hit-testing

  let SelectedCenter = null;
  let SelectionLocked = false;

  let CustomSelection = null;
  let CustomModeActive = false;

  // Add at top-level of DetectionModules
  let TemplateTracking = {
      templateMat: null,
      originalWidth: 0,
      originalHeight: 0,
      dotnetHelper: null,
      enabled: false
  };

  console.log("✅ DetectionModules script loaded.");

  function enableCustomCrop(overlayIdOrElement) {
      // Accept either an element or an ID
      const overlay = typeof overlayIdOrElement === "string" ? document.getElementById(overlayIdOrElement) : overlayIdOrElement;
      if (!overlay) {
          console.warn("enableCustomCrop: overlay is null");
          return;
      }

      // avoid attaching twice
      if (overlay._customCropAttached) return;
      overlay._customCropAttached = true;

      let rect = null;
      let startX = 0;
      let startY = 0;
      let dragging = false; // ✅ declare properly

      // set module state
      CustomModeActive = true;
      SelectionMode = "custom";

      function getCanvasPos(e) {
          const r = overlay.getBoundingClientRect();
          const clientX = (e.touches ? e.touches[0].clientX : e.clientX);
          const clientY = (e.touches ? e.touches[0].clientY : e.clientY);

          const x = Math.round((clientX - r.left) * (overlay.width / r.width));
          const y = Math.round((clientY - r.top) * (overlay.height / r.height));
          return { x, y };
      }

      function onDown(e) {
          e.preventDefault();
          const p = getCanvasPos(e);
          startX = p.x;
          startY = p.y;
          rect = { x: startX, y: startY, w: 0, h: 0 };
          dragging = true;
      }

      function onMove(e) {
          if (!dragging) return;
          e.preventDefault();
          const p = getCanvasPos(e);
          rect.x = Math.min(startX, p.x);
          rect.y = Math.min(startY, p.y);
          rect.w = Math.abs(p.x - startX);
          rect.h = Math.abs(p.y - startY);
          drawSelectionRect(overlay, rect);
      }

      function onUp(e) {
          if (!dragging) return;
          e.preventDefault();
          dragging = false;
          draggingFinished(rect);
      }

      function drawSelectionRect(canvas, rect) {
          const ctx = canvas.getContext("2d");
          ctx.clearRect(0, 0, canvas.width, canvas.height);
          ctx.beginPath();
          ctx.lineWidth = 2;
          ctx.setLineDash([6, 4]);
          ctx.strokeStyle = "white";
          ctx.rect(rect.x, rect.y, rect.w, rect.h); // ✅ use rect correctly
          ctx.stroke();
          ctx.setLineDash([]);
      }

      function draggingFinished(finalRect) {
          CustomSelection = finalRect;
          console.log("CustomSelection:", CustomSelection);
          drawSelectionRect(overlay, CustomSelection);
      }

      overlay.addEventListener("mousedown", onDown);
      overlay.addEventListener("mousemove", onMove);
      overlay.addEventListener("mouseup", onUp);

      overlay.addEventListener("touchstart", onDown);
      overlay.addEventListener("touchmove", onMove);
      overlay.addEventListener("touchend", onUp);

      // allow disabling custom crop mode
      if (!window._customCrop) window._customCrop = {};
      window._customCrop.disable = () => {
          CustomModeActive = false;
          SelectionMode = "discover";
          overlay._customCropAttached = false;

          // remove listeners by replacing overlay
          const clone = overlay.cloneNode(true);
          overlay.parentNode.replaceChild(clone, overlay);
          console.log("🔴 Custom crop mode disabled.");
      };

      console.log("🟢 Custom crop mode enabled.");
  }

  // 🟦 Enable clicking on overlay to select target
  function enableClickSelection(overlayCanvas) {
    if (!overlayCanvas) {
      console.warn("enableClickSelection: overlayCanvas is null");
      return;
    }

    // avoid attaching twice
    if (overlayCanvas.__clickAttached) return;
    overlayCanvas.__clickAttached = true;

    overlayCanvas.addEventListener("click", (e) => {
      const rect = overlayCanvas.getBoundingClientRect();
      const x = Math.round(e.clientX - rect.left);
      const y = Math.round(e.clientY - rect.top);

      console.log("🖱️ CLICK DETECTED", { x, y, rectWidth: rect.width, rectHeight: rect.height });
      console.log("Latest detections (count):", latestDetections.length);

      if (!latestDetections || latestDetections.length === 0) {
        console.log("⚠ No detections available yet.");
      }

      let hit = false;
      for (let d of latestDetections) {
        // using inclusive bounds
        const inBox =
          x >= Math.round(d.rect.x) &&
          x <= Math.round(d.rect.x + d.rect.width) &&
          y >= Math.round(d.rect.y) &&
          y <= Math.round(d.rect.y + d.rect.height);

        console.log(`Check ID=${d.id}: rect=(${Math.round(d.rect.x)},${Math.round(d.rect.y)},${Math.round(d.rect.width)},${Math.round(d.rect.height)}) hit=${inBox}`);

        // Inside overlay click
    if (inBox) {
        hit = true;

        SelectedCenter = { x: d.centerX, y: d.centerY };
        SelectionLocked = true;
        SelectionMode = "focus";

        console.log("Mode:", SelectionMode);

        // --- Calculate your offsets & angle ---
        let offsetX1 = Math.round(d.centerX - overlayCanvas.width / 2);
        let offsetY1 = Math.round(d.centerY - overlayCanvas.height / 2);
        let rotation1 = Math.round(d.angle || 0); // depends on your detection pipeline

        console.log("📤 Sending to Razor:", offsetX1, offsetY1, rotation1);

        // --- Send TO C# Razor page ---
        if (window.mainPipeline && window.mainPipeline._dotnetHelper) {
            window.mainPipeline._dotnetHelper.invokeMethodAsync(
                "OnTargetOffsetsUpdated",
                offsetX1,
                offsetY1,
                rotation1
            )
            .catch(err => console.warn("JS→C# send failed:", err));
        }

        break;
    }

      }

      if (!hit) console.log("❌ No target hit.");
    });

    console.log("enableClickSelection: listener attached to overlayCanvas");
  }

  // 🟩 Capture Module: gets frame from video element
  function CaptureModule(videoId) {
    const video = document.getElementById(videoId);
    if (!video) throw new Error("Video element not found: " + videoId);

    console.log("🎥 CaptureModule initialized on", videoId);

    // reusable offscreen canvas for performance
    const tempCanvas = document.createElement("canvas");
    const tempCtx = tempCanvas.getContext("2d");

    return {
      getFrame: function () {
        if (!video.videoWidth || !video.videoHeight) return null;

        if (tempCanvas.width !== video.videoWidth || tempCanvas.height !== video.videoHeight) {
          tempCanvas.width = video.videoWidth;
          tempCanvas.height = video.videoHeight;
        }

        tempCtx.drawImage(video, 0, 0);
        const src = cv.imread(tempCanvas);
        return src;
      }
    };
  }

  // 🟨 ObjectDetectionModule: generic shape/contour detection
  function ObjectDetectionModule(detectionType) {
      console.log("🔍 ObjectDetectionModule initialized with type:", detectionType);

      function detectRectangles(frame) {
          if (!frame || frame.empty()) return [];

          let gray = new cv.Mat();
          cv.cvtColor(frame, gray, cv.COLOR_RGBA2GRAY);
          cv.GaussianBlur(gray, gray, new cv.Size(5, 5), 0);
          let edges = new cv.Mat();
          cv.Canny(gray, edges, 75, 150);

          let contours = new cv.MatVector();
          let hierarchy = new cv.Mat();
          cv.findContours(edges, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);

          let results = [];

          for (let i = 0; i < contours.size(); i++) {
              let cnt = contours.get(i);
              let rect = cv.boundingRect(cnt);
              let area = cv.contourArea(cnt);

              // skip small noise
              if (area < 500) {
                  cnt.delete();
                  continue;
              }

              let width = rect.width;
              let height = rect.height;
              let centerX = rect.x + width / 2;
              let centerY = rect.y + height / 2;

              // classify: square vs rectangle
              let shapeType = "rectangle";
              let ratio = width / height;
              if (ratio > 0.9 && ratio < 1.1) {
                  shapeType = "square";
              }

              let rotatedRect = cv.minAreaRect(cnt);
              let angle = rotatedRect.angle;
              if (angle < -45) angle += 90;

              results.push({
                  id: uniqueIdCounter++,
                  rect,
                  centerX,
                  centerY,
                  area,
                  rotation: angle,
                  shape: shapeType
              });

              cnt.delete();
          }

          gray.delete();
          edges.delete();
          contours.delete();
          hierarchy.delete();

          return results;
      }

      function detectCircles(frame) {
          if (!frame || frame.empty()) return [];

          let gray = new cv.Mat();
          cv.cvtColor(frame, gray, cv.COLOR_RGBA2GRAY);
          cv.GaussianBlur(gray, gray, new cv.Size(9, 9), 2, 2);

          let circles = new cv.Mat();
          cv.HoughCircles(
              gray,
              circles,
              cv.HOUGH_GRADIENT,
              1,                  // dp
              30,                 // minDist between circles
              120,                // param1 = canny threshold
              40,                 // param2 = center detection threshold
              10,                 // minRadius
              200                 // maxRadius
          );

          let results = [];

          for (let i = 0; i < circles.cols; ++i) {
              let x = circles.data32F[i * 3];
              let y = circles.data32F[i * 3 + 1];
              let r = circles.data32F[i * 3 + 2];

              results.push({
                  id: uniqueIdCounter++,
                  rect: {
                      x: x - r,
                      y: y - r,
                      width: r * 2,
                      height: r * 2
                  },
                  centerX: x,
                  centerY: y,
                  radius: r,
                  area: Math.PI * r * r,
                  rotation: 0
              });
          }

          gray.delete();
          circles.delete();

          return results;
      }

        let uniqueIdCounter = 0; // global incremental ID

        // ---------------- QR CODE DETECTION ----------------
        function detectQRCode(frame, ctxOverlay = null) {
            if (!frame || frame.empty()) return [];

            const results = [];
            let idCounter = uniqueIdCounter;

            try {
                let qr = new cv.QRCodeDetector();
                let qrPoints = new cv.Mat();
                let straight = new cv.Mat();

                let okQR = qr.detect(frame, qrPoints);
                if (okQR && qrPoints.rows > 0) {
                    // decode QR text
                    let qrText = qr.decode(frame, qrPoints);

                    const p = qrPoints.data32F;
                    const xMin = Math.min(p[0], p[2], p[4], p[6]);
                    const yMin = Math.min(p[1], p[3], p[5], p[7]);
                    const xMax = Math.max(p[0], p[2], p[4], p[6]);
                    const yMax = Math.max(p[1], p[3], p[5], p[7]);

                    const width = xMax - xMin;
                    const height = yMax - yMin;
                    const cx = xMin + width / 2;
                    const cy = yMin + height / 2;

                    results.push({
                        id: idCounter++,
                        type: "qr",
                        rect: { x: xMin, y: yMin, width, height },
                        centerX: cx,
                        centerY: cy,
                        rotation: 0,
                        area: width * height,
                        text: qrText
                    });

                    // Draw overlay
                    if (ctxOverlay) {
                        ctxOverlay.strokeStyle = "lime";
                        ctxOverlay.lineWidth = 2;
                        ctxOverlay.strokeRect(xMin, yMin, width, height);
                        ctxOverlay.fillStyle = "lime";
                        ctxOverlay.fillText(`QR: ${qrText}`, xMin, yMin - 5);
                    }
                }

                qr.delete();
                qrPoints.delete();
                straight.delete();
            } catch (err) {
                console.warn("QR detection error:", err);
            }

            uniqueIdCounter = idCounter;
            return results;
        }

        function detectBarcodeContours(frame, ctxOverlay = null) {
            if (!frame || frame.empty()) return [];

            const results = [];
            let idCounter = uniqueIdCounter;

            let gray = new cv.Mat();
            cv.cvtColor(frame, gray, cv.COLOR_RGBA2GRAY);

            // Apply Sobel X to detect vertical stripes
            let gradX = new cv.Mat();
            cv.Sobel(gray, gradX, cv.CV_32F, 1, 0, 3);

            cv.convertScaleAbs(gradX, gradX);

            // Blur + threshold
            let blur = new cv.Mat();
            cv.GaussianBlur(gradX, blur, new cv.Size(9, 9), 0);
            let thresh = new cv.Mat();
            cv.threshold(blur, thresh, 128, 255, cv.THRESH_BINARY | cv.THRESH_OTSU);

            // Morphological closing to connect barcode lines
            let kernel = cv.getStructuringElement(cv.MORPH_RECT, new cv.Size(21, 7));
            cv.morphologyEx(thresh, thresh, cv.MORPH_CLOSE, kernel);

            // Find contours
            let contours = new cv.MatVector();
            let hierarchy = new cv.Mat();
            cv.findContours(thresh, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);

            for (let i = 0; i < contours.size(); i++) {
                let cnt = contours.get(i);
                let rect = cv.boundingRect(cnt);

                if (rect.width < 50 || rect.height < 20) { cnt.delete(); continue; } // filter small

                const cx = rect.x + rect.width / 2;
                const cy = rect.y + rect.height / 2;

                results.push({
                    id: idCounter++,
                    type: "barcode",
                    codeType: "1D",
                    rect: rect,
                    centerX: cx,
                    centerY: cy,
                    rotation: 0,
                    area: rect.width * rect.height,
                    text: "" // can't decode here yet
                });

                if (ctxOverlay) {
                    ctxOverlay.strokeStyle = "orange";
                    ctxOverlay.lineWidth = 2;
                    ctxOverlay.strokeRect(rect.x, rect.y, rect.width, rect.height);
                    ctxOverlay.fillStyle = "orange";
                    ctxOverlay.fillText("1D barcode", rect.x, rect.y - 5);
                }

                cnt.delete();
            }

            gradX.delete(); blur.delete(); gray.delete(); thresh.delete(); contours.delete(); hierarchy.delete(); kernel.delete();

            uniqueIdCounter = idCounter;
            return results;
        }

        // ---------------- RUN BOTH ----------------
        function detectAllCodes(frame, ctxOverlay = null) {
            const qrResults = detectQRCode(frame, ctxOverlay);
            const barResults = detectBarcodeContours(frame, ctxOverlay);
            return [...qrResults, ...barResults];
        }

        return {
            detectObjects(frame, currentType) {
                currentType = currentType || detectionType; // fallback to original
                console.log("🔍 Using detectionType:", currentType);
                if (currentType === "circle") return detectCircles(frame);
                if (currentType === "qr") return detectAllCodes(frame);
                return detectRectangles(frame);
            }
        };

  }

  // 🟦 AnalysisModule: computes center offset, rotation, etc.
  function CenterRotationAnalysisModule() {
    console.log("📐 CenterRotationAnalysisModule initialized.");

    return {
      analyze: function (detections, frameWidth, frameHeight) {
        let screenCX = frameWidth / 2;
        let screenCY = frameHeight / 2;

        return detections.map(d => {
            let offsetX = d.centerX - screenCX;
            let offsetY = screenCY - d.centerY;
            return {
                ...d,
                offsetX,
                offsetY,
                onScreen:
                d.rect.x >= 0 &&
                d.rect.y >= 0 &&
                d.rect.x + d.rect.width <= frameWidth &&
                d.rect.y + d.rect.height <= frameHeight,
                rotation: d.rotation
            };
        });
      }
    };
  }

  // 🟥 MainPipeline: ties everything together
  function MainPipelineModule(videoId, dotnetHelper, detectionType) {
      console.log("🚀 MainPipelineModule initialized on", videoId);

      const cap = CaptureModule(videoId);
      const detector = ObjectDetectionModule(detectionType);
      const analyzer = CenterRotationAnalysisModule();
      const overlay = document.getElementById("overlayCanvas");
      const ctx = overlay.getContext("2d");

      overlay.width = document.getElementById(videoId).videoWidth;
      overlay.height = document.getElementById(videoId).videoHeight;

      this._dotnetHelper = dotnetHelper;
      let running = false;

      function syncOverlaySize() {
          const vid = document.getElementById(videoId);
          if (vid && vid.videoWidth && vid.videoHeight) {
              overlay.width = vid.videoWidth;
              overlay.height = vid.videoHeight;
              overlay.style.width = vid.clientWidth + "px";
              overlay.style.height = vid.clientHeight + "px";
          }
      }

      syncOverlaySize();
      enableClickSelection(overlay);
      const vid = document.getElementById(videoId);
      vid.addEventListener("loadedmetadata", syncOverlaySize);
      window.addEventListener("resize", syncOverlaySize);

      function loop() {
          if (!running) return;

          let frame = cap.getFrame();
          if (!frame) {
              requestAnimationFrame(loop);
              return;
          }

          let detections = [];

          // --- CUSTOM CROP LIVE-TRACKING ---
          if (TemplateTracking.enabled && TemplateTracking.templateMat) {
              const searchW = TemplateTracking.originalWidth * 2;
              const searchH = TemplateTracking.originalHeight * 2;

              let startX = Math.max(0, SelectedCenter.x - searchW / 2);
              let startY = Math.max(0, SelectedCenter.y - searchH / 2);
              startX = Math.min(startX, frame.cols - searchW);
              startY = Math.min(startY, frame.rows - searchH);

              let roi = frame.roi(new cv.Rect(startX, startY, searchW, searchH));
              let result = new cv.Mat();
              cv.matchTemplate(roi, TemplateTracking.templateMat, result, cv.TM_CCOEFF_NORMED);
              let minMax = cv.minMaxLoc(result);
              let maxLoc = minMax.maxLoc;

              let x = startX + maxLoc.x;
              let y = startY + maxLoc.y;
              let centerX = x + TemplateTracking.originalWidth / 2;
              let centerY = y + TemplateTracking.originalHeight / 2;

              SelectedCenter = { x: centerX, y: centerY };
              SelectionLocked = true;
              SelectionMode = "focus";

              // draw template overlay
              ctx.strokeStyle = "red";
              ctx.lineWidth = 2;
              ctx.strokeRect(centerX - TemplateTracking.originalWidth / 2,
                            centerY - TemplateTracking.originalHeight / 2,
                            TemplateTracking.originalWidth,
                            TemplateTracking.originalHeight);

              // send offset to .NET
              if (TemplateTracking.dotnetHelper) {
                  const offsetX = centerX - frame.cols / 2;
                  const offsetY = (frame.rows / 2 - centerY);
                  TemplateTracking.dotnetHelper.invokeMethodAsync("UpdateCropInfo", offsetX, offsetY, 0);
              }

              detections.push({
                  id: 9999, // unique ID for template tracking
                  rect: { x, y, width: TemplateTracking.originalWidth, height: TemplateTracking.originalHeight },
                  centerX,
                  centerY,
                  rotation: 0
              });

              roi.delete();
              result.delete();
          } else {
              // --- NORMAL DETECTION ---
              detections = detector.detectObjects(frame);

              // Focus mode if a target was clicked
              if (SelectionLocked && SelectedCenter) {
                  let best = null;
                  let bestDist = Infinity;
                  detections.forEach(d => {
                      const dx = d.centerX - SelectedCenter.x;
                      const dy = d.centerY - SelectedCenter.y;
                      const dist = Math.sqrt(dx*dx + dy*dy);
                      if (dist < bestDist && dist < 80) {
                          best = d;
                          bestDist = dist;
                      }
                  });
                  if (best) {
                      detections = [best];
                      SelectedCenter = { x: best.centerX, y: best.centerY };
                  } else {
                      detections = [];
                  }
              }
          }

          latestDetections = detections;

          let analyzed = analyzer.analyze(detections, frame.cols, frame.rows);

          // draw all detections
          ctx.clearRect(0, 0, overlay.width, overlay.height);
          analyzed.forEach(d => {
              ctx.strokeStyle = (SelectionMode === "focus") ? "lime" : "cyan";
              ctx.lineWidth = 2;
              ctx.beginPath();
              ctx.rect(d.rect.x, d.rect.y, d.rect.width, d.rect.height);
              ctx.stroke();

              ctx.fillStyle = (SelectionMode === "focus") ? "red" : "orange";
              ctx.beginPath();
              ctx.arc(d.centerX, d.centerY, 4, 0, Math.PI * 2);
              ctx.fill();

              ctx.fillStyle = "yellow";
              ctx.font = "14px Arial";
              ctx.fillText(`X:${Math.round(d.offsetX)} Y:${Math.round(d.offsetY)} R:${Math.round(d.rotation)}`, 
                          d.rect.x, d.rect.y - 5);
          });

          // send to .NET
          if (dotnetHelper) {
              dotnetHelper.invokeMethodAsync("ReceiveDetections", analyzed).catch(e => console.warn(e));
          }

          frame.delete();
          requestAnimationFrame(loop);
      }

      return {
          start: () => { running = true; requestAnimationFrame(loop); },
          stop: () => { running = false; }
      };
  }

  // ✅ Return all modules
  return {
    CaptureModule,
    ObjectDetectionModule,
    CenterRotationAnalysisModule,
    MainPipelineModule,
    enableCustomCrop,
    startPipeline: function(videoId, dotnetHelper, detectionType) {
        if (window.mainPipeline) window.mainPipeline.stop();
        window.mainPipeline = new MainPipelineModule(videoId, dotnetHelper, detectionType);
        window.mainPipeline.start();
    },
    stopPipeline: function() {
        if (window.mainPipeline) {
            try {
                window.mainPipeline.stop();
                window.mainPipeline = null; // clear reference
                console.log("🛑 Detection pipeline stopped.");
            } catch (err) {
                console.warn("⚠ Failed to stop pipeline:", err);
            }
        }
    },
    // Inside window.DetectionModules
    getSelectedTargetImage: function() {
        if (!SelectionLocked || !SelectedCenter) return null;

        const video = document.getElementById("videoElement");
        if (!video || video.videoWidth === 0 || video.videoHeight === 0) return null;

        const overlay = document.getElementById("overlayCanvas");
        const ctxTemp = document.createElement("canvas").getContext("2d");
        const tempCanvas = document.createElement("canvas");

        tempCanvas.width = video.videoWidth;
        tempCanvas.height = video.videoHeight;

        const tempCtx = tempCanvas.getContext("2d");
        tempCtx.drawImage(video, 0, 0, tempCanvas.width, tempCanvas.height);

        // Find the best matched detection for the selected center
        let best = null;
        let bestDist = Infinity;
        latestDetections.forEach(d => {
            const dx = d.centerX - SelectedCenter.x;
            const dy = d.centerY - SelectedCenter.y;
            const dist = Math.sqrt(dx*dx + dy*dy);
            if (dist < bestDist && dist < 80) {
                best = d;
                bestDist = dist;
            }
        });

        if (!best) return null;

        // Crop the selected target rectangle
        const { x, y, width, height } = best.rect;
        const cropCanvas = document.createElement("canvas");
        cropCanvas.width = width;
        cropCanvas.height = height;
        const cropCtx = cropCanvas.getContext("2d");
        cropCtx.drawImage(tempCanvas, x, y, width, height, 0, 0, width, height);

        return cropCanvas.toDataURL("image/png"); // return as base64
    },

    getSelectedTargetCrop: function(targetWidthPx, targetHeightPx) {
        if (!(SelectionLocked && SelectedCenter)) return null;

        const video = document.getElementById("videoElement");
        if (!video || video.videoWidth === 0 || video.videoHeight === 0) return null;

        // Convert target width/height to pixels if needed
        const w = targetWidthPx;
        const h = targetHeightPx;

        // Calculate top-left of crop based on center
        let sx = SelectedCenter.x - w/2;
        let sy = SelectedCenter.y - h/2;

        // Clamp to video bounds
        sx = Math.max(0, Math.min(sx, video.videoWidth - w));
        sy = Math.max(0, Math.min(sy, video.videoHeight - h));

        const offCanvas = document.createElement("canvas");
        offCanvas.width = w;
        offCanvas.height = h;
        const ctx = offCanvas.getContext("2d");
        ctx.drawImage(video, sx, sy, w, h, 0, 0, w, h);

        const base64 = offCanvas.toDataURL("image/png");

        // store in sessionStorage
        sessionStorage.setItem("selectedTargetImage", base64);

        return { base64, sx, sy, width: w, height: h };
    },

    getCustomCrop: function(targetWidthPx, targetHeightPx) {
      if (!CustomSelection) return null;
      const video = document.getElementById("videoElement");
      if (!video || video.videoWidth === 0) return null;

      const sx = Math.max(0, Math.min(CustomSelection.x, video.videoWidth));
      const sy = Math.max(0, Math.min(CustomSelection.y, video.videoHeight));
      const sw = Math.max(1, Math.min(CustomSelection.w, video.videoWidth - sx));
      const sh = Math.max(1, Math.min(CustomSelection.h, video.videoHeight - sy));

      const outW = targetWidthPx || sw;
      const outH = targetHeightPx || sh;
      const off = document.createElement("canvas");
      off.width = outW;
      off.height = outH;

      const ctx = off.getContext("2d");
      ctx.drawImage(video, sx, sy, sw, sh, 0, 0, outW, outH);
      return off.toDataURL("image/png");
    },

    uploadCustomCrop: async function (width, height) {
        try {
            const base64 = this.getCustomCrop(width, height);
            if (!base64) return { success: false, message: "No custom selection" };

            const byteString = atob(base64.split(',')[1]);
            const mimeString = base64.split(',')[0].split(':')[1].split(';')[0];
            const buffer = new ArrayBuffer(byteString.length);
            const u8arr = new Uint8Array(buffer);
            for (let i = 0; i < byteString.length; i++) u8arr[i] = byteString.charCodeAt(i);

            const blob = new Blob([buffer], { type: mimeString });
            const formData = new FormData();
            formData.append("file", blob, "crop.png");

            const resp = await fetch("/api/image/upload", { method: "POST", body: formData });

            if (!resp.ok) {
                const text = await resp.text();
                console.error("uploadCustomCrop server error", resp.status, resp.statusText, text);
                return { success: false, message: `Server error ${resp.status}: ${text}` };
            }

            const result = await resp.json();
            return { success: true, fileUrl: result.fileUrl, size: result.size };
        } catch (err) {
            console.error("uploadCustomCrop JS error", err);
            return { success: false, message: err?.message ?? "Unknown JS error" };
        }
    },

    lockCustomCropForDetection: function(dotnetHelperRef) {
        if (!CustomSelection) {
            console.warn("No custom selection available!");
            return;
        }

        const video = document.getElementById("videoElement");
        if (!video || video.videoWidth === 0 || video.videoHeight === 0) return;

        // Initialize TemplateTracking ROI from CustomSelection
        const sx = Math.max(0, Math.min(CustomSelection.x, video.videoWidth));
        const sy = Math.max(0, Math.min(CustomSelection.y, video.videoHeight));
        const sw = Math.max(1, Math.min(CustomSelection.w, video.videoWidth - sx));
        const sh = Math.max(1, Math.min(CustomSelection.h, video.videoHeight - sy));

        // Capture initial template
        const tempCanvas = document.createElement("canvas");
        tempCanvas.width = sw;
        tempCanvas.height = sh;
        const ctx = tempCanvas.getContext("2d");
        ctx.drawImage(video, sx, sy, sw, sh, 0, 0, sw, sh);
        const imgData = ctx.getImageData(0, 0, sw, sh);
        TemplateTracking.templateMat = cv.matFromImageData(imgData);
        TemplateTracking.originalWidth = sw;
        TemplateTracking.originalHeight = sh;
        TemplateTracking.dotnetHelper = dotnetHelperRef;
        TemplateTracking.enabled = true;

        // ------------------------------------------------------------
        // NEW: calibration values (only defined, not computed yet)
        TemplateTracking.knownDistance = 10;     // physical cm
        TemplateTracking.realObjectWidth = 5;    // physical cm
        TemplateTracking.focalLength = null;     // computed later
        TemplateTracking.basePixelWidth = null;  // pixel width at known distance
        // ------------------------------------------------------------

        SelectedCenter = { x: sx + sw / 2, y: sy + sh / 2 };
        SelectionLocked = true;
        SelectionMode = "focus";

        console.log("🔒 Template tracking started:", sw, sh);

        function trackingLoop() {
            if (!TemplateTracking.enabled) return;

            const cap = DetectionModules.CaptureModule("videoElement");
            let frame = cap.getFrame();
            if (!frame) {
                requestAnimationFrame(trackingLoop);
                return;
            }

            // Crop search area around previous SelectedCenter
            const searchW = TemplateTracking.originalWidth * 2;
            const searchH = TemplateTracking.originalHeight * 2;
            let startX = Math.max(0, SelectedCenter.x - searchW / 2);
            let startY = Math.max(0, SelectedCenter.y - searchH / 2);
            startX = Math.min(startX, frame.cols - searchW);
            startY = Math.min(startY, frame.rows - searchH);

            let roi = frame.roi(new cv.Rect(startX, startY, searchW, searchH));

            // Template matching
            let result = new cv.Mat();
            cv.matchTemplate(roi, TemplateTracking.templateMat, result, cv.TM_CCOEFF_NORMED);
            let minMax = cv.minMaxLoc(result);
            let maxLoc = minMax.maxLoc;

            let newX = startX + maxLoc.x;
            let newY = startY + maxLoc.y;
            let centerX = newX + TemplateTracking.originalWidth / 2;
            let centerY = newY + TemplateTracking.originalHeight / 2;

            SelectedCenter = { x: centerX, y: centerY };

            // Compute rotation & contour width
            let rotation = 0;
            let currentWidth = TemplateTracking.originalWidth;
            let rect = null;

            try {
                let gray = new cv.Mat();
                cv.cvtColor(roi, gray, cv.COLOR_RGBA2GRAY);

                let thresh = new cv.Mat();
                cv.threshold(gray, thresh, 128, 255, cv.THRESH_BINARY | cv.THRESH_OTSU);

                let contours = new cv.MatVector();
                let hierarchy = new cv.Mat();
                cv.findContours(thresh, contours, hierarchy, cv.RETR_EXTERNAL, cv.CHAIN_APPROX_SIMPLE);

                if (contours.size() > 0) {
                    let maxArea = 0, maxIdx = 0;
                    for (let i = 0; i < contours.size(); i++) {
                        let cnt = contours.get(i);
                        let area = cv.contourArea(cnt);
                        if (area > maxArea) {
                            maxArea = area;
                            maxIdx = i;
                        }
                    }
                    let cnt = contours.get(maxIdx);
                    rect = cv.minAreaRect(cnt);
                    rotation = rect.angle < -45 ? rect.angle + 90 : rect.angle;

                    currentWidth = Math.max(rect.size.width, rect.size.height);
                    cnt.delete();
                }

                gray.delete();
                thresh.delete();
                contours.delete();
                hierarchy.delete();
            } catch (err) {
                console.warn("Rotation / contour computation failed:", err);
            }

            // ------------------------------------------------------------
            // CALIBRATION FIX: compute focal length only once with real contour size
            if (rect && rect.size && !TemplateTracking.focalLength) {
                TemplateTracking.basePixelWidth = currentWidth;

                TemplateTracking.focalLength =
                    (TemplateTracking.basePixelWidth * TemplateTracking.knownDistance) /
                    TemplateTracking.realObjectWidth;

                console.log("📐 Focal length calculated:", TemplateTracking.focalLength);
                console.log("📌 Base pixel width:", TemplateTracking.basePixelWidth);
            }
            // ------------------------------------------------------------

            // Compute offsets
            const offsetX = centerX - frame.cols / 2;
            const offsetY = frame.rows / 2 - centerY;

            // Compute distance using calibrated focal
            let distanceCm = 0;
            if (TemplateTracking.focalLength) {
                distanceCm =
                    (TemplateTracking.realObjectWidth * TemplateTracking.focalLength) /
                    currentWidth;
            }

            // Send offsets + rotation
            if (TemplateTracking.dotnetHelper) {
                TemplateTracking.dotnetHelper.invokeMethodAsync("UpdateCropInfo", offsetX, offsetY, rotation);
            }

            // Draw overlay
            const ctxOverlay = document.getElementById("overlayCanvas").getContext("2d");
            ctxOverlay.clearRect(0, 0, frame.cols, frame.rows);

            ctxOverlay.strokeStyle = "red";
            ctxOverlay.lineWidth = 2;
            ctxOverlay.strokeRect(
                centerX - TemplateTracking.originalWidth / 2,
                centerY - TemplateTracking.originalHeight / 2,
                TemplateTracking.originalWidth,
                TemplateTracking.originalHeight
            );

            ctxOverlay.fillStyle = "red";
            ctxOverlay.beginPath();
            ctxOverlay.arc(centerX, centerY, 4, 0, Math.PI * 2);
            ctxOverlay.fill();

            ctxOverlay.fillStyle = "yellow";
            ctxOverlay.font = "14px Arial";
            ctxOverlay.fillText(
                `X:${Math.round(offsetX)} Y:${Math.round(offsetY)} R:${Math.round(rotation)}`,
                newX,
                newY - 5
            );

            ctxOverlay.fillStyle = "cyan";
            ctxOverlay.font = "16px Arial";
            ctxOverlay.fillText(`Distance: ${Math.round(distanceCm)}cm`, 10, 20);

            roi.delete();
            result.delete();
            frame.delete();

            requestAnimationFrame(trackingLoop);
        }

        trackingLoop();
    }

  };
})();