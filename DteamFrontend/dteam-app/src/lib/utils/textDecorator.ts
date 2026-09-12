import { BACKEND_URL } from './constants';

export function escapeHtml(str: string): string {
  if (!str) return '';
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

export function resolveMediaUrl(url: string): string {
  if (!url) return '';
  const trimmed = url.trim();
  if (trimmed.startsWith('http://') || trimmed.startsWith('https://') || trimmed.startsWith('blob:') || trimmed.startsWith('data:')) {
    return trimmed;
  }
  const cleanPath = trimmed.startsWith('/') ? trimmed : `/${trimmed}`;
  return BACKEND_URL ? `${BACKEND_URL.replace(/\/+$/, '')}${cleanPath}` : cleanPath;
}

export interface RenderDecoratedOptions {
  preserveNewlines?: boolean;
  allowImages?: boolean;
}

export function renderDecoratedText(raw?: string | null, options: RenderDecoratedOptions = {}): string {
  if (!raw) return '';
  const { preserveNewlines = true, allowImages = true } = options;

  let safe = escapeHtml(raw);

  safe = safe.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

  safe = safe.replace(/(?<!\*)\*([^*]+?)\*(?!\*)/g, '<em>$1</em>');

  safe = safe.replace(/&lt;u&gt;(.*?)&lt;\/u&gt;/gi, '<u>$1</u>');

  safe = safe.replace(/\[u\](.*?)\[\/u\]/gi, '<u>$1</u>');

  if (allowImages) {
    safe = safe.replace(/!\[(.*?)\]\((.*?)\)/g, (_match, alt, url) => {
      const trimmed = (url || '').trim();
      if (
        trimmed.startsWith('https://') ||
        trimmed.startsWith('http://') ||
        trimmed.startsWith('/uploads/') ||
        trimmed.startsWith('/api/media/') ||
        trimmed.startsWith('/api/chat/media/')
      ) {
        const resolved = resolveMediaUrl(trimmed);
        return `<img src="${resolved}" alt="${alt}" class="rounded-xl max-h-80 w-auto my-2 object-cover border border-cyan-900/60" loading="lazy" decoding="async" onerror="this.style.display='none'" />`;
      }
      return '';
    });
  }

  if (preserveNewlines) {
    safe = safe.replace(/\n/g, '<br />');
  }

  return safe;
}
