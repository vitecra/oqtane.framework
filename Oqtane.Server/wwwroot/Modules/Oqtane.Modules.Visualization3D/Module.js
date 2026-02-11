window.Visualization3D = {
    canvases: {},

    initialize: function(canvasId, width, height, backgroundColor) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) {
            console.error('Canvas not found:', canvasId);
            return false;
        }

        const ctx = canvas.getContext('2d');
        if (!ctx) {
            console.error('Could not get 2D context');
            return false;
        }

        canvas.width = width;
        canvas.height = height;

        this.canvases[canvasId] = {
            canvas: canvas,
            ctx: ctx,
            width: width,
            height: height,
            rotation: 0
        };

        this.clear(canvasId, backgroundColor);
        return true;
    },

    clear: function(canvasId, backgroundColor) {
        const canvasData = this.canvases[canvasId];
        if (!canvasData) return;

        const ctx = canvasData.ctx;
        ctx.fillStyle = backgroundColor;
        ctx.fillRect(0, 0, canvasData.width, canvasData.height);
    },

    drawCube: function(canvasId, position, scale, rotation, color) {
        const canvasData = this.canvases[canvasId];
        if (!canvasData) return;

        const ctx = canvasData.ctx;
        const centerX = canvasData.width / 2;
        const centerY = canvasData.height / 2;

        // Simple 2D projection of a 3D cube
        const size = scale.x * 50; // Scale factor for visibility
        const x = centerX + position.x * 50;
        const y = centerY - position.y * 50;

        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(rotation.y + canvasData.rotation);

        // Draw cube faces (simplified wireframe)
        ctx.strokeStyle = color;
        ctx.lineWidth = 2;

        // Front face
        ctx.strokeRect(-size / 2, -size / 2, size, size);

        // Back face (offset)
        const offset = size * 0.3;
        ctx.strokeRect(-size / 2 + offset, -size / 2 - offset, size, size);

        // Connect corners
        ctx.beginPath();
        ctx.moveTo(-size / 2, -size / 2);
        ctx.lineTo(-size / 2 + offset, -size / 2 - offset);
        ctx.moveTo(size / 2, -size / 2);
        ctx.lineTo(size / 2 + offset, -size / 2 - offset);
        ctx.moveTo(-size / 2, size / 2);
        ctx.lineTo(-size / 2 + offset, size / 2 - offset);
        ctx.moveTo(size / 2, size / 2);
        ctx.lineTo(size / 2 + offset, size / 2 - offset);
        ctx.stroke();

        ctx.restore();
    },

    drawSphere: function(canvasId, position, radius, color) {
        const canvasData = this.canvases[canvasId];
        if (!canvasData) return;

        const ctx = canvasData.ctx;
        const centerX = canvasData.width / 2;
        const centerY = canvasData.height / 2;

        const x = centerX + position.x * 50;
        const y = centerY - position.y * 50;
        const r = radius * 50;

        ctx.save();
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(x, y, r, 0, 2 * Math.PI);
        ctx.fill();
        ctx.strokeStyle = color;
        ctx.stroke();
        ctx.restore();
    },

    drawPlane: function(canvasId, position, width, height, rotation, color) {
        const canvasData = this.canvases[canvasId];
        if (!canvasData) return;

        const ctx = canvasData.ctx;
        const centerX = canvasData.width / 2;
        const centerY = canvasData.height / 2;

        const x = centerX + position.x * 50;
        const y = centerY - position.y * 50;
        const w = width * 50;
        const h = height * 50;

        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(rotation.z);
        ctx.fillStyle = color;
        ctx.fillRect(-w / 2, -h / 2, w, h);
        ctx.strokeStyle = color;
        ctx.strokeRect(-w / 2, -h / 2, w, h);
        ctx.restore();
    },

    updateRotation: function(canvasId, delta) {
        const canvasData = this.canvases[canvasId];
        if (!canvasData) return;
        canvasData.rotation += delta;
    },

    dispose: function(canvasId) {
        delete this.canvases[canvasId];
    }
};
