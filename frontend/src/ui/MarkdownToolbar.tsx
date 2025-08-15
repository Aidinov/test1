import { Button, ButtonGroup } from '@mui/material';

interface Props {
  onInsert: (snippet: string) => void;
}

export default function MarkdownToolbar({ onInsert }: Props) {
  const insert = (snippet: string) => () => onInsert(snippet);
  return (
    <ButtonGroup size="small" sx={{ mb: 1 }}>
      <Button aria-label="bold" onClick={insert('**bold**')}>B</Button>
      <Button aria-label="italic" onClick={insert('_italic_')}>I</Button>
      <Button aria-label="code" onClick={insert('`code`')}>{'<>'}</Button>
      <Button aria-label="link" onClick={insert('[title](url)')}>Link</Button>
      <Button aria-label="mermaid" onClick={insert('\n```mermaid\n<diagram>\n```\n')}>Mermaid</Button>
    </ButtonGroup>
  );
}
