import { useEffect, useRef } from 'react';
import DOMPurify from 'dompurify';
import { marked } from 'marked';

export function sanitizeHtmlWithIframeWhitelist(html: string, whitelist: string[]): string {
  const clean = DOMPurify.sanitize(html, {
    ADD_TAGS: ['iframe'],
    ADD_ATTR: ['allow', 'allowfullscreen', 'frameborder', 'src', 'referrerpolicy'],
  });

  const parser = new DOMParser();
  const doc = parser.parseFromString(clean, 'text/html');
  const frames = doc.querySelectorAll('iframe');
  frames.forEach((frame) => {
    const src = frame.getAttribute('src') || '';
    let allowed = false;
    try {
      const url = new URL(src, 'https://example.com');
      allowed = whitelist.some(
        (host) => url.hostname === host || url.hostname.endsWith(`.${host}`)
      );
    } catch {
      allowed = false;
    }
    if (!allowed) {
      const placeholder = doc.createElement('div');
      placeholder.textContent = '[iframe blocked]';
      frame.replaceWith(placeholder);
    }
  });
  return doc.body.innerHTML;
}

const DEFAULT_IFRAME_WHITELIST = ['youtube.com', 'youtu.be', 'figma.com', 'diagrams.net'];

export default function Markdown({ markdown }: { markdown: string }) {
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const renderer = new marked.Renderer();
    const originalCode = renderer.code.bind(renderer);
    renderer.code = (code, infostring, escaped) => {
      const lang = (infostring || '').trim();
      if (lang === 'mermaid') {
        const escapedCode = code
          .replace(/&/g, '&amp;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;');
        return `<div class="mermaid">${escapedCode}</div>`;
      }
      return originalCode(code, infostring, escaped);
    };

    const raw = marked(markdown, { renderer });
    const sanitized = sanitizeHtmlWithIframeWhitelist(raw, DEFAULT_IFRAME_WHITELIST);
    if (ref.current) {
      ref.current.innerHTML = sanitized;
    }

    const blocks = ref.current?.querySelectorAll<HTMLDivElement>('div.mermaid') ?? [];
    if (blocks.length > 0) {
      import('mermaid')
        .then((m) => {
          m.default.initialize({ startOnLoad: false });
          blocks.forEach((block, idx) => {
            const code = block.textContent || '';
            m.default.render(`mermaid-${idx}`, code).then(({ svg }) => {
              block.innerHTML = svg;
            });
          });
        })
        .catch(() => {});
    }
  }, [markdown]);

  return <div ref={ref} />;
}

