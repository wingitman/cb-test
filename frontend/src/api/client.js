const API_BASE_URL = (import.meta.env.PUBLIC_API_BASE_URL || 'https://localhost:7072').replace(/\/$/, '');

export class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

export async function apiRequest(path, { signal, ...options } = {}) {
  const controller = new AbortController();
  const timeout = setTimeout(() => controller.abort(), 10000);

  if (signal) {
    signal.addEventListener('abort', () => controller.abort(), { once: true });
  }

  try {
    const response = await fetch(`${API_BASE_URL}${path}`, {
      ...options,
      headers: {
        Accept: 'application/json',
        ...options.headers,
      },
      signal: controller.signal,
    });

    if (!response.ok) {
      let message = `Request failed with status ${response.status}.`;
      try {
        const body = await response.json();
        if (body.message) message = body.message;
      } catch {
        // Keep the status-based message when the server has no JSON error body.
      }

      throw new ApiError(message, response.status);
    }

    return response.json();
  } catch (error) {
    if (error.name === 'AbortError') {
      throw error;
    }

    if (error instanceof ApiError) {
      throw error;
    }

    throw new ApiError('Unable to connect to the products service.');
  } finally {
    clearTimeout(timeout);
  }
}
