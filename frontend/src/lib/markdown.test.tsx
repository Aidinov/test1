import { render, waitFor } from '@testing-library/react';
import Markdown from './markdown';
import { afterEach, describe, expect, test, vi } from 'vitest';

vi.mock('mermaid', () => ({
  default: {
    initialize: vi.fn(),
    render: vi.fn().mockResolvedValue({ svg: '<svg></svg>' }),
  },
}));

afterEach(() => {
  document.body.innerHTML = '';
});

describe('Markdown renderer', () => {
  test('script tags are stripped', () => {
    const { container } = render(
      <Markdown markdown={'<script>alert(1)</script>'} />
    );
    expect(container.querySelector('script')).toBeNull();
  });

  test('non-whitelisted iframes are replaced', () => {
    const { getByText, container } = render(
      <Markdown markdown={'<iframe src="https://evil.com"></iframe>'} />
    );
    expect(container.querySelector('iframe')).toBeNull();
    expect(getByText('[iframe blocked]')).toBeTruthy();
  });

  test('mermaid blocks render to svg', async () => {
    const md = '```mermaid\ngraph TD;A-->B;\n```';
    const { container } = render(<Markdown markdown={md} />);
    await waitFor(() => {
      expect(container.querySelector('svg')).not.toBeNull();
    });
  });
});

