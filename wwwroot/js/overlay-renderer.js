export const OverlayRenderer = {
    init(videoId, canvasId) {
        this.video = document.getElementById(videoId);
        this.canvas = document.getElementById(canvasId);
        this.ctx = this.canvas.getContext("2d");

        const syncSize = () => {
            if (!this.video.videoWidth) return;
            this.canvas.width = this.video.videoWidth;
            this.canvas.height = this.video.videoHeight;

            this.canvas.style.width = this.video.clientWidth + "px";
            this.canvas.style.height = this.video.clientHeight + "px";

            console.log("Canvas resized:", this.canvas.width, this.canvas.height);
        };

        this.video.onloadedmetadata = syncSize;

        // 🔥 The real event where video is definitely ready
        this.video.addEventListener("playing", syncSize);

        window.addEventListener("resize", syncSize);
    },

    render(det) {
        if (!this.ctx) return;

        console.log("Canvas:", this.canvas.width, this.canvas.height,
                    "Video:", this.video.videoWidth, this.video.videoHeight);

        if (this.canvas.width === 0 || this.canvas.height === 0)
            return;

        const ctx = this.ctx;

        // scale drawing to match display size
        ctx.setTransform(
            this.canvas.clientWidth / this.canvas.width,
            0,
            0,
            this.canvas.clientHeight / this.canvas.height,
            0,
            0
        );

        ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);

        // --- crosshair ---
        const midX = this.canvas.width / 2;
        const midY = this.canvas.height / 2;

        ctx.strokeStyle = "white";
        ctx.lineWidth = 2;

        ctx.beginPath();
        ctx.moveTo(midX - 30, midY);
        ctx.lineTo(midX + 30, midY);
        ctx.stroke();

        ctx.beginPath();
        ctx.moveTo(midX, midY - 30);
        ctx.lineTo(midX, midY + 30);
        ctx.stroke();

        ctx.fillStyle = "white";
        ctx.beginPath();
        ctx.arc(midX, midY, 4, 0, Math.PI * 2);
        ctx.fill();

        if (!det) return;

        ctx.lineWidth = 3;
        ctx.strokeStyle = "lime";
        ctx.strokeRect(det.x, det.y, det.width, det.height);

        ctx.fillStyle = "red";
        ctx.beginPath();
        ctx.arc(det.centerX, det.centerY, 5, 0, Math.PI * 2);
        ctx.fill();
    }
};
