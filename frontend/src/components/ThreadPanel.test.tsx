import { render } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import ThreadPanel from './ThreadPanel';
import { useStore } from '../store';

describe('ThreadPanel', () => {
  it('renders threads', () => {
    useStore.setState({ threads: [{ id: '1', text: 'Test', status: 'Open' }] });
    const { getByText } = render(<ThreadPanel />);
    expect(getByText('Test - Open')).toBeTruthy();
  });
});
