import { apiRequest } from './client';

export function getProducts(filters, signal) {
  const params = new URLSearchParams();

  if (filters.search.trim()) {
    params.set('search', filters.search.trim());
  }

  if (filters.category) {
    params.set('category', filters.category);
  }

  if (filters.inventory) {
    params.set('inventory', filters.inventory);
  }

  if (filters.location) {
    params.set('location', filters.location);
  }



  const query = params.toString();
  return apiRequest(`/products${query ? `?${query}` : ''}`, { signal });
}

export function getLocations(signal) {
  return apiRequest('/locations', { signal });
}

export function getCategories(signal) {
  return apiRequest('/categories', { signal });
}

export function updateProduct(productId, product) {
  return apiRequest(`/products/${productId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(product),
  });
}

export function addInventory(productId, inventory) {
  return apiRequest(`/products/${productId}/inventory`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(inventory),
  });
}

export function updateInventory(inventoryId, inventory) {
  return apiRequest(`/inventory/${inventoryId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(inventory),
  });
}

export function toggleProductActive(productId) {
  return apiRequest(`/product/${productId}/active`, {
    method: 'PUT',
  })
}

export function deleteInventory(inventoryId) {
  return apiRequest(`/inventory/${inventoryId}`, { method: 'DELETE' });
}

export function addLocation(location) {
  return apiRequest('/locations', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(location),
  });
}

export function updateLocation(locationId, location) {
  return apiRequest(`/locations/${locationId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(location),
  });
}

export function deleteLocation(locationId) {
  return apiRequest(`/locations/${locationId}`, { method: 'DELETE' });
}
