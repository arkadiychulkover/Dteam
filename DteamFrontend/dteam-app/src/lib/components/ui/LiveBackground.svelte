<script lang="ts">
  import { onMount } from 'svelte';

  let canvas: HTMLCanvasElement;
  let mouse = { x: -1000, y: -1000, targetX: -1000, targetY: -1000 };

  type ShapeType = 'blob' | 'triangle' | 'hexagon' | 'diamond' | 'ring' | 'star' | 'pill';

  interface GeoShape {
    x: number;
    y: number;
    vx: number;
    vy: number;
    baseRadius: number;
    radius: number;
    angle: number;
    angleSpeed: number;
    pulseSpeed: number;
    pulsePhase: number;
    type: ShapeType;
    color: string;
    glowColor: string;
    vertices?: { baseAngle: number; noiseOffset: number; speed: number }[];
  }

  onMount(() => {
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    let animationFrameId: number;
    let width = (canvas.width = window.innerWidth);
    let height = (canvas.height = window.innerHeight);

    const handleResize = () => {
      width = canvas.width = window.innerWidth;
      height = canvas.height = window.innerHeight;
    };

    const handleMouseMove = (e: MouseEvent) => {
      mouse.targetX = e.clientX;
      mouse.targetY = e.clientY;
    };

    const handleTouchMove = (e: TouchEvent) => {
      if (e.touches.length > 0) {
        mouse.targetX = e.touches[0].clientX;
        mouse.targetY = e.touches[0].clientY;
      }
    };

    window.addEventListener('resize', handleResize);
    window.addEventListener('mousemove', handleMouseMove);
    window.addEventListener('touchmove', handleTouchMove);

    const palette = [
      { color: 'rgba(13, 242, 201, 0.85)', glow: 'rgba(13, 242, 201, 1)' },
      { color: 'rgba(0, 242, 254, 0.85)', glow: 'rgba(0, 242, 254, 1)' },
      { color: 'rgba(168, 85, 247, 0.8)', glow: 'rgba(168, 85, 247, 1)' },
      { color: 'rgba(217, 70, 239, 0.75)', glow: 'rgba(217, 70, 239, 1)' },
      { color: 'rgba(16, 185, 129, 0.8)', glow: 'rgba(16, 185, 129, 1)' },
      { color: 'rgba(59, 130, 246, 0.8)', glow: 'rgba(59, 130, 246, 1)' },
      { color: 'rgba(244, 63, 94, 0.75)', glow: 'rgba(244, 63, 94, 1)' },
    ];

    const shapeTypes: ShapeType[] = ['blob', 'triangle', 'hexagon', 'diamond', 'ring', 'star', 'pill'];

    const count = Math.max(12, Math.min(20, Math.floor((width * height) / 50000)));
    const shapes: GeoShape[] = Array.from({ length: count }, (_, i) => {
      const p = palette[i % palette.length];
      const type = shapeTypes[i % shapeTypes.length];
      const baseR = Math.random() * 140 + 100;

      const s: GeoShape = {
        x: Math.random() * width,
        y: Math.random() * height,
        vx: (Math.random() - 0.5) * 1.2,
        vy: (Math.random() - 0.5) * 1.2,
        baseRadius: baseR,
        radius: baseR,
        angle: Math.random() * Math.PI * 2,
        angleSpeed: (Math.random() - 0.5) * 0.015,
        pulseSpeed: Math.random() * 0.02 + 0.008,
        pulsePhase: Math.random() * Math.PI * 2,
        type,
        color: p.color,
        glowColor: p.glow,
      };

      if (type === 'blob') {
        const vertexCount = 8;
        s.vertices = Array.from({ length: vertexCount }, (_, idx) => ({
          baseAngle: (idx / vertexCount) * Math.PI * 2,
          noiseOffset: Math.random() * Math.PI * 2,
          speed: Math.random() * 0.03 + 0.01,
        }));
      }

      return s;
    });

    let time = 0;

    const drawPolygon = (cx: number, cy: number, r: number, sides: number, angle: number) => {
      ctx.beginPath();
      for (let i = 0; i < sides; i++) {
        const a = angle + (i * 2 * Math.PI) / sides;
        const px = cx + Math.cos(a) * r;
        const py = cy + Math.sin(a) * r;
        if (i === 0) ctx.moveTo(px, py);
        else ctx.lineTo(px, py);
      }
      ctx.closePath();
    };

    const drawStar = (cx: number, cy: number, outerR: number, innerR: number, points: number, angle: number) => {
      ctx.beginPath();
      for (let i = 0; i < points * 2; i++) {
        const a = angle + (i * Math.PI) / points;
        const r = i % 2 === 0 ? outerR : innerR;
        const px = cx + Math.cos(a) * r;
        const py = cy + Math.sin(a) * r;
        if (i === 0) ctx.moveTo(px, py);
        else ctx.lineTo(px, py);
      }
      ctx.closePath();
    };

    const drawBlob = (cx: number, cy: number, r: number, shape: GeoShape, t: number) => {
      if (!shape.vertices) return;
      const v = shape.vertices;
      const pts = v.map((vert) => {
        const offset = Math.sin(t * vert.speed + vert.noiseOffset) * (r * 0.35);
        const curR = r + offset;
        const curAngle = vert.baseAngle + shape.angle;
        return {
          x: cx + Math.cos(curAngle) * curR,
          y: cy + Math.sin(curAngle) * curR,
        };
      });

      ctx.beginPath();
      ctx.moveTo((pts[0].x + pts[v.length - 1].x) / 2, (pts[0].y + pts[v.length - 1].y) / 2);
      for (let i = 0; i < v.length; i++) {
        const next = pts[(i + 1) % v.length];
        const midX = (pts[i].x + next.x) / 2;
        const midY = (pts[i].y + next.y) / 2;
        ctx.quadraticCurveTo(pts[i].x, pts[i].y, midX, midY);
      }
      ctx.closePath();
    };

    const render = () => {
      time += 0.015;

      mouse.x += (mouse.targetX - mouse.x) * 0.05;
      mouse.y += (mouse.targetY - mouse.y) * 0.05;

      ctx.fillStyle = '#02090d';
      ctx.fillRect(0, 0, width, height);

      shapes.forEach((s, idx) => {
        s.vx += Math.sin(time * 0.6 + idx * 1.9) * 0.04;
        s.vy += Math.cos(time * 0.5 + idx * 2.5) * 0.04;

        s.vx *= 0.985;
        s.vy *= 0.985;

        s.x += s.vx;
        s.y += s.vy;
        s.angle += s.angleSpeed;

        const pad = s.baseRadius * 1.5;
        if (s.x < -pad) s.x = width + pad;
        if (s.x > width + pad) s.x = -pad;
        if (s.y < -pad) s.y = height + pad;
        if (s.y > height + pad) s.y = -pad;

        s.radius = s.baseRadius * (1 + Math.sin(time * s.pulseSpeed + s.pulsePhase) * 0.25);

        if (mouse.x > 0 && mouse.y > 0) {
          const dx = s.x - mouse.x;
          const dy = s.y - mouse.y;
          const dist = Math.sqrt(dx * dx + dy * dy);
          if (dist < 350 && dist > 0) {
            const force = (1 - dist / 350) * 1.8;
            s.x += (dx / dist) * force;
            s.y += (dy / dist) * force;
          }
        }

        ctx.save();
        ctx.shadowColor = s.glowColor;
        ctx.shadowBlur = 60;

        const radial = ctx.createRadialGradient(s.x, s.y, 0, s.x, s.y, s.radius * 1.3);
        radial.addColorStop(0, s.glowColor);
        radial.addColorStop(0.4, s.color);
        radial.addColorStop(1, 'transparent');

        ctx.fillStyle = radial;

        switch (s.type) {
          case 'blob':
            drawBlob(s.x, s.y, s.radius, s, time);
            break;
          case 'triangle':
            drawPolygon(s.x, s.y, s.radius, 3, s.angle);
            break;
          case 'hexagon':
            drawPolygon(s.x, s.y, s.radius, 6, s.angle);
            break;
          case 'diamond':
            drawPolygon(s.x, s.y, s.radius, 4, s.angle);
            break;
          case 'star':
            drawStar(s.x, s.y, s.radius, s.radius * 0.45, 5, s.angle);
            break;
          case 'pill':
            ctx.save();
            ctx.translate(s.x, s.y);
            ctx.rotate(s.angle);
            ctx.beginPath();
            ctx.roundRect(-s.radius * 0.9, -s.radius * 0.45, s.radius * 1.8, s.radius * 0.9, s.radius * 0.4);
            ctx.restore();
            break;
          case 'ring':
            ctx.beginPath();
            ctx.arc(s.x, s.y, s.radius, 0, Math.PI * 2);
            ctx.arc(s.x, s.y, s.radius * 0.5, 0, Math.PI * 2, true);
            ctx.closePath();
            break;
        }

        ctx.fill();
        ctx.restore();
      });

      if (mouse.x > 0 && mouse.y > 0) {
        const mouseRadial = ctx.createRadialGradient(mouse.x, mouse.y, 0, mouse.x, mouse.y, 400);
        mouseRadial.addColorStop(0, 'rgba(13, 242, 201, 0.35)');
        mouseRadial.addColorStop(0.5, 'rgba(0, 242, 254, 0.12)');
        mouseRadial.addColorStop(1, 'transparent');
        ctx.fillStyle = mouseRadial;
        ctx.beginPath();
        ctx.arc(mouse.x, mouse.y, 400, 0, Math.PI * 2);
        ctx.fill();
      }

      animationFrameId = requestAnimationFrame(render);
    };

    render();

    return () => {
      cancelAnimationFrame(animationFrameId);
      window.removeEventListener('resize', handleResize);
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('touchmove', handleTouchMove);
    };
  });
</script>

<div class="fixed inset-0 pointer-events-none z-0 overflow-hidden">
  <canvas 
    bind:this={canvas} 
    class="absolute inset-0 w-full h-full filter blur-[60px] sm:blur-[90px] scale-110 transform"
  ></canvas>

  <div class="absolute inset-0 bg-[#02090d]/55"></div>

  <div class="absolute inset-0 bg-[radial-gradient(circle_at_50%_30%,transparent_0%,#010609_90%)] opacity-65"></div>

  <div class="absolute inset-0 opacity-[0.03] bg-[radial-gradient(#0df2c9_1px,transparent_1px)] [background-size:24px_24px]"></div>
</div>
