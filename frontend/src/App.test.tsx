import { render, screen } from '@testing-library/react'
import { describe, it, expect, beforeEach } from 'vitest'
import App from './App'

beforeEach(() => {
  globalThis.fetch = (() => Promise.resolve({ json: () => Promise.resolve([]) })) as any;
});

describe('App', () => {
  it('renders the title', () => {
    render(<App />);
    expect(screen.getByText('Indkøbsliste')).toBeInTheDocument();
  });

  it('renders the add input', () => {
    render(<App />);
    expect(screen.getByPlaceholderText('Tilføj vare...')).toBeInTheDocument();
  });

  it('shows empty state', async () => {
    render(<App />);
    expect(await screen.findByText('Listen er tom')).toBeInTheDocument();
  });
});
