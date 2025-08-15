import { selectionToOffsets } from './selectionToOffsets';
import { describe, it, expect } from 'vitest';

describe('selectionToOffsets', () => {
  it('handles duplicate substrings by mapping to the correct occurrence', () => {
    const markdown = 'alpha beta alpha';
    const root = document.createElement('div');
    // split into two text nodes to mimic real DOM
    root.append(
      document.createTextNode('alpha beta '),
      document.createTextNode('alpha')
    );
    document.body.appendChild(root);
    const range = document.createRange();
    const second = root.childNodes[1];
    range.setStart(second, 0);
    range.setEnd(second, 5);
    const res = selectionToOffsets(markdown, range, root);
    expect(res).toEqual({ startIndex: 11, endIndex: 16 });
  });
});
