window.currentStream = null;
window.selectedCameraName = null;

window.startCamera = async function (deviceId) {
    try {
        // Stop existing camera before switching
        if (window.currentStream) {
            window.currentStream.getTracks().forEach(track => track.stop());
        }

        // Ask permission if not yet granted
        await navigator.mediaDevices.getUserMedia({ video: true });

        // Try to use the selected camera
        let constraints = { video: true };
        if (deviceId && deviceId !== "default") {
            constraints = { video: { deviceId: { exact: deviceId } } };
        }

        let stream = null;
        try {
            stream = await navigator.mediaDevices.getUserMedia(constraints);
        } catch (ex) {
            console.warn("⚠️ Could not use exact deviceId, falling back to default:", ex);
            // fallback if browser rejected the exact match
            stream = await navigator.mediaDevices.getUserMedia({ video: true });
        }

        const videoElement = document.getElementById('videoElement');
        videoElement.srcObject = stream;

        window.currentStream = stream;
        console.log("✅ Camera started:", deviceId || "(default)");
    } catch (err) {
        console.error("❌ Error starting camera:", err);
        alert("Failed to start camera: " + err.message);
    }
};

window.getCameraList = async function () {
    // Request permission so that camera labels become visible
    await navigator.mediaDevices.getUserMedia({ video: true });

    const devices = await navigator.mediaDevices.enumerateDevices();
    const cameras = devices
        .filter(d => d.kind === 'videoinput')
        .map(d => ({
            id: d.deviceId,
            label: d.label || `Camera ${d.deviceId}`
        }));

    console.log("🎥 Available cameras:", cameras);
    return cameras;
};

function getTimestamp() {
    const now = new Date();
    const pad = n => n.toString().padStart(2, '0');
    return `${now.getFullYear()}-${pad(now.getMonth()+1)}-${pad(now.getDate())}_${pad(now.getHours())}-${pad(now.getMinutes())}-${pad(now.getSeconds())}`;
}

window.takePhoto = async function () {
    if (!window.currentStream) {
        console.warn("No camera stream active");
        return null;
    }

    const video = document.getElementById('videoElement');
    const canvas = document.createElement('canvas');
    canvas.width = video.videoWidth || 640;
    canvas.height = video.videoHeight || 480;
    canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);

    const timestamp = getTimestamp();
    const fileName = `photo_${timestamp}.png`;
    const imageData = canvas.toDataURL("image/png");

    try {
        const res = await fetch('/api/capture/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                imageBase64: imageData,
                fileName: fileName,
                cameraName: window.selectedCameraName || "UnknownCamera"
            })
        });

        if (!res.ok) {
            console.error("❌ Photo upload failed", res.status);
            return null;
        }

        const data = await res.json();
        console.log("📸 Photo saved:", data.path);

        return data.path; // ✅ THIS IS THE KEY
    }
    catch (err) {
        console.error("Error saving photo:", err);
        return null;
    }
};

window.startRecording = function () {
    const stream = window.currentStream;
    if (!stream) {
        console.error("❌ No active video stream!");
        return;
    }

    window.mediaRecorder = new MediaRecorder(stream, { mimeType: 'video/webm' });
    window.recordedChunks = [];

    window.mediaRecorder.ondataavailable = (e) => {
        if (e.data.size > 0) {
            window.recordedChunks.push(e.data);
        }
    };

    window.mediaRecorder.start();
    console.log("⏺️ Recording started...");
};

window.stopRecording = async function () {
    if (!window.mediaRecorder) {
        console.error("❌ No recording!");
        return null;
    }

    return new Promise(resolve => {
        window.mediaRecorder.onstop = async () => {
            const blob = new Blob(window.recordedChunks, { type: 'video/webm' });
            const timestamp = getTimestamp();
            const fileName = `video_${timestamp}.webm`;

            const formData = new FormData();
            formData.append("videoFile", blob, fileName);
            formData.append("cameraName", window.selectedCameraName || "UnknownCamera");

            try {
                const res = await fetch('/api/capture/savevideo', {
                    method: 'POST',
                    body: formData
                });
                if (!res.ok) {
                    console.error("❌ Video upload failed", res.status);
                    resolve(null);
                    return;
                }
                const data = await res.json();
                console.log("✅ Video saved:", data.path);
                resolve(data.path);
            } catch (err) {
                console.error("Error saving video:", err);
                resolve(null);
            }
        };

        window.mediaRecorder.stop();
    });
};

window.pauseRecording = function () {
    if (!window.mediaRecorder) {
        console.error("❌ No active recording to pause!");
        return;
    }

    if (window.mediaRecorder.state === "recording") {
        window.mediaRecorder.pause();
        console.log("⏸️ Recording paused.");
    } else {
        console.warn("⚠️ Recording is not in progress or already paused.");
    }
};

window.resumeRecording = function () {
    if (!window.mediaRecorder) {
        console.error("❌ No active recording to resume!");
        return;
    }

    if (window.mediaRecorder.state === "paused") {
        window.mediaRecorder.resume();
        console.log("▶️ Recording resumed.");
    } else {
        console.warn("⚠️ Recording is not paused, cannot resume.");
    }
};
