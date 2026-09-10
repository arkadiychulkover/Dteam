export function escapeHtml(str: string): string {
  if (!str) return '';
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

export interface RenderDecoratedOptions {
  preserveNewlines?: boolean;
  allowImages?: boolean;
}

export function renderDecoratedText(raw?: string | null, options: RenderDecoratedOptions = {}): string {
  if (!raw) return '';
  const { preserveNewlines = true, allowImages = true } = options;

  let safe = escapeHtml(raw);

  // Bold: **text**
  safe = safe.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');

  // Italic: *text* (ensure not preceded or followed by *)
  safe = safe.replace(/(?<!\*)\*([^*]+?)\*(?!\*)/g, '<em>$1</em>');

  // Underline: HTML <u>...</u> (escaped to &lt;u&gt;...&lt;/u&gt;)
  safe = safe.replace(/&lt;u&gt;(.*?)&lt;\/u&gt;/gi, '<u>$1</u>');

  // Underline: BBCode [u]...[/u]
  safe = safe.replace(/\[u\](.*?)\[\/u\]/gi, '<u>$1</u>');

  // Markdown images: ![alt](url)
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
        return `<img src="${trimmed}" alt="${alt}" class="rounded-xl max-h-80 w-auto my-2 object-cover border border-cyan-900/60" loading="lazy" decoding="async" />`;
      }
      return '';
    });
  }

  // Newlines
  if (preserveNewlines) {
    safe = safe.replace(/\n/g, '<br />');
  }

  return safe;
}

