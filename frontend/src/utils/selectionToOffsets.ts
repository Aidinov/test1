export interface Offsets {
  startIndex: number;
  endIndex: number;
}

/**
 * Compute absolute offsets of a DOM Range within the source markdown.
 * Avoids substring searches so duplicate selections map correctly.
 */
export function selectionToOffsets(
  markdown: string,
  range: Range,
  root: HTMLElement
): Offsets | null {
  if (range.collapsed) return null;
  const pre = range.cloneRange();
  pre.selectNodeContents(root);
  pre.setEnd(range.startContainer, range.startOffset);
  const start = pre.toString().length;
  pre.setEnd(range.endContainer, range.endOffset);
  const end = pre.toString().length;
  if (end > markdown.length) return null;
  return { startIndex: start, endIndex: end };
}
