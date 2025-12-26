window.CheckTarget = {
    drawImageWithCenterPoint: (canvasId, imageUrl, offsetX = 0, offsetY = 0, angle = 0, maxWidth = 400) => {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const ctx = canvas.getContext("2d");
        const img = new Image();
        img.src = imageUrl;

        img.onload = () => {
            let scale = 1;
            if (img.width > maxWidth) scale = maxWidth / img.width;

            canvas.width = img.width * scale;
            canvas.height = img.height * scale;

            ctx.clearRect(0, 0, canvas.width, canvas.height);
            ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

            const cx = canvas.width / 2 + offsetX * scale;
            const cy = canvas.height / 2 + offsetY * scale;

            // Red center point
            ctx.fillStyle = "red";
            ctx.beginPath();
            ctx.arc(cx, cy, 5, 0, 2 * Math.PI);
            ctx.fill();

            // Crosshair lines
            ctx.strokeStyle = "red";
            ctx.lineWidth = 1;

            // Horizontal
            ctx.beginPath();
            ctx.moveTo(0, cy);
            ctx.lineTo(canvas.width, cy);
            ctx.stroke();

            // Vertical
            ctx.beginPath();
            ctx.moveTo(cx, 0);
            ctx.lineTo(cx, canvas.height);
            ctx.stroke();

            // Rotation line
            const len = Math.min(canvas.width, canvas.height) / 2;
            const rad = angle * Math.PI / 180;
            const x1 = cx - len * Math.cos(rad);
            const y1 = cy - len * Math.sin(rad);
            const x2 = cx + len * Math.cos(rad);
            const y2 = cy + len * Math.sin(rad);

            ctx.strokeStyle = "lime";
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(x1, y1);
            ctx.lineTo(x2, y2);
            ctx.stroke();

            // Optional: show offset text
            // ctx.fillStyle = "yellow";
            // ctx.font = "14px Arial";
            // ctx.fillText(`OffsetX: ${offsetX}`, 10, 20);
            // ctx.fillText(`OffsetY: ${offsetY}`, 10, 40);
            // ctx.fillText(`Angle: ${angle}°`, 10, 60);
        };
    }
};
