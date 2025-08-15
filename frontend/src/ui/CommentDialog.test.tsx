import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import CommentDialog from './CommentDialog';

describe('CommentDialog', () => {
  it('snapshots open/close and shows validation errors', () => {
    const handleSubmit = vi.fn();
    const { asFragment, rerender } = render(
      <CommentDialog open={false} onClose={() => {}} onSubmit={handleSubmit} />
    );
    expect(asFragment()).toMatchSnapshot(); // closed

    rerender(<CommentDialog open={true} onClose={() => {}} onSubmit={handleSubmit} />);
    expect(asFragment()).toMatchSnapshot(); // open

    fireEvent.click(screen.getByText('Save'));
    expect(asFragment()).toMatchSnapshot(); // validation error
  });
});
