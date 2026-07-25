import { expect, test } from '@rstest/core';
import { cleanup, fireEvent, render, screen } from '@testing-library/react';
import App from '../src/App';

const product = {
  id: 'product-1',
  name: 'NVIDIA GeForce RTX 4070',
  sku: 'GPU-4070-NV',
  isActive: true,
  category: { id: 'category-1', name: 'GPU' },
};

test('loads and displays products from the API', async () => {
  const originalFetch = globalThis.fetch;
  globalThis.fetch = async () => new Response(JSON.stringify([product]), { status: 200 });

  try {
    render(<App />);

    expect(await screen.findByText(product.name)).toBeInTheDocument();
    expect(screen.getByText(product.sku)).toBeInTheDocument();
    expect(screen.getAllByText(product.category.name).length).toBeGreaterThan(1);
  } finally {
    cleanup();
    globalThis.fetch = originalFetch;
  }
});

test('sends search and category filters to the API', async () => {
  const originalFetch = globalThis.fetch;
  const requests = [];
  globalThis.fetch = async (url) => {
    requests.push(url);
    return new Response(JSON.stringify([]), { status: 200 });
  };

  try {
    render(<App />);
    await screen.findByText('No products match the current filters.');

    fireEvent.change(screen.getByLabelText('Search products'), { target: { value: 'RTX' } });
    fireEvent.change(screen.getByLabelText('Category'), { target: { value: 'GPU' } });
    await new Promise((resolve) => globalThis.setTimeout(resolve, 500));

    const filteredRequest = requests.at(-1);
    expect(filteredRequest).toContain('search=RTX');
    expect(filteredRequest).toContain('category=GPU');
  } finally {
    cleanup();
    globalThis.fetch = originalFetch;
  }
});

test('shows an API error', async () => {
  const originalFetch = globalThis.fetch;
  globalThis.fetch = async () => new Response('', { status: 503 });

  try {
    render(<App />);

    expect(await screen.findByRole('alert')).toHaveTextContent('Products could not be loaded.');
  } finally {
    cleanup();
    globalThis.fetch = originalFetch;
  }
});
