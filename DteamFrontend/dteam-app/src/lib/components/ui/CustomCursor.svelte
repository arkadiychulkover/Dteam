<script lang="ts">
  import { onMount } from 'svelte';

  let dotEl: HTMLDivElement | undefined = $state();
  let ringEl: HTMLDivElement | undefined = $state();

  let isHovering = $state(false);
  let isClicking = $state(false);
  let isHidden = $state(true);

  let mouseX = 0;
  let mouseY = 0;
  let ringX = 0;
  let ringY = 0;
  let animId: number | null = null;

  onMount(() => {
    // Only activate for mouse/fine pointer devices
    if (!window.matchMedia('(pointer: fine)').matches) {
      return;
    }

    document.body.classList.add('hidecur');

    mouseX = window.innerWidth / 2;
    mouseY = window.innerHeight / 2;
    ringX = mouseX;
    ringY = mouseY;

    const handleMouseMove = (e: MouseEvent) => {
      mouseX = e.clientX;
      mouseY = e.clientY;
      if (isHidden) isHidden = false;
    };

    const handleMouseDown = () => {
      isClicking = true;
    };

    const handleMouseUp = () => {
      isClicking = false;
    };

    const handleMouseLeave = () => {
      isHidden = true;
    };

    const handleMouseEnter = () => {
      isHidden = false;
    };

    // Event delegation for interactive elements (dynamically covers all SPA routes)
    const handleMouseOver = (e: MouseEvent) => {
      const target = (e.target as HTMLElement)?.closest(
        'a, button, input, textarea, select, [role="button"], .cursor-pointer, label, summary, [tabindex]:not([tabindex="-1"])'
      );
      isHovering = !!target;
    };

    window.addEventListener('mousemove', handleMouseMove, { passive: true });
    window.addEventListener('mousedown', handleMouseDown, { passive: true });
    window.addEventListener('mouseup', handleMouseUp, { passive: true });
    document.addEventListener('mouseleave', handleMouseLeave);
    document.addEventListener('mouseenter', handleMouseEnter);
    document.addEventListener('mouseover', handleMouseOver, { passive: true });

    // Smooth physics trailing loop identical to NULLSPREAD
    const renderLoop = () => {
      ringX += (mouseX - ringX) * 0.18;
      ringY += (mouseY - ringY) * 0.18;

      if (dotEl) {
        dotEl.style.transform = `translate3d(${mouseX - 3}px, ${mouseY - 3}px, 0)`;
      }

      if (ringEl) {
        const halfSize = ringEl.offsetWidth / 2;
        const scale = isClicking ? ' scale(0.82)' : '';
        ringEl.style.transform = `translate3d(${ringX - halfSize}px, ${ringY - halfSize}px, 0)${scale}`;
      }

      animId = requestAnimationFrame(renderLoop);
    };

    animId = requestAnimationFrame(renderLoop);

    return () => {
      document.body.classList.remove('hidecur');
      window.removeEventListener('mousemove', handleMouseMove);
      window.removeEventListener('mousedown', handleMouseDown);
      window.removeEventListener('mouseup', handleMouseUp);
      document.removeEventListener('mouseleave', handleMouseLeave);
      document.removeEventListener('mouseenter', handleMouseEnter);
      document.removeEventListener('mouseover', handleMouseOver);
      if (animId) cancelAnimationFrame(animId);
    };
  });
</script>

<div
  bind:this={dotEl}
  id="custom-cursor-dot"
  class:cursor-hidden={isHidden}
  aria-hidden="true"
></div>

<div
  bind:this={ringEl}
  id="custom-cursor-ring"
  class:cursor-hover={isHovering}
  class:cursor-hidden={isHidden}
  aria-hidden="true"
></div>
