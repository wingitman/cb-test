import React, { useEffect, useState } from 'react';
import {
  addInventory,
  addLocation,
  deleteInventory,
  deleteLocation,
  getLocations,
  getProducts,
  toggleProductActive,
  updateInventory,
  updateLocation,
} from './api/products';
import './App.css';

const initialFilters = { search: '', category: '' };
const categories = ['GPU', 'CPU', 'Motherboard', 'RAM', 'Storage', 'Mouse', 'Keyboard', 'Monitor'];
const currency = new Intl.NumberFormat('en-GB', { style: 'currency', currency: 'GBP' });

function InventoryForm({ inventory, locations, onCancel, onSubmit }) {
  const [locationId, setLocationId] = useState(inventory?.location?.id || locations[0]?.id || '');
  const [amount, setAmount] = useState(inventory?.amount || '');
  const [capacityCost, setCapacityCost] = useState(inventory?.capacityCost || '');

  const submit = (event) => {
    event.preventDefault();
    onSubmit({ locationId, amount: Number(amount), capacityCost: Number(capacityCost) });
  };

  return (
    <form className="editor-form" onSubmit={submit}>
      <label>
        Location
        <select value={locationId} onChange={(event) => setLocationId(event.target.value)} required>
          <option value="" disabled>
            Select a location
          </option>
          {locations.map((location) => (
            <option key={location.id} value={location.id}>
              {location.name}
            </option>
          ))}
        </select>
      </label>
      <label>
        Amount
        <input type="number" min="1" value={amount} onChange={(event) => setAmount(event.target.value)} required />
      </label>
      <label>
        Capacity cost
        <input
          type="number"
          min="1"
          value={capacityCost}
          onChange={(event) => setCapacityCost(event.target.value)}
          required
        />
      </label>
      <div className="editor-actions">
        <button type="submit" className="primary-button">
          {inventory ? 'Save inventory' : 'Add inventory'}
        </button>
        {onCancel && (
          <button type="button" className="secondary-button" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}

function LocationForm({ location, onCancel, onSubmit }) {
  const [form, setForm] = useState({
    name: location?.name || '',
    region: location?.region || '',
    country: location?.country || '',
    capacity: location?.capacity || '',
  });

  const update = (name, value) => setForm((current) => ({ ...current, [name]: value }));

  return (
    <form
      className="location-form"
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit({ ...form, capacity: Number(form.capacity) });
      }}
    >
      <label>
        Name
        <input value={form.name} onChange={(event) => update('name', event.target.value)} required />
      </label>
      <label>
        Region
        <input value={form.region} onChange={(event) => update('region', event.target.value)} required />
      </label>
      <label>
        Country
        <input value={form.country} onChange={(event) => update('country', event.target.value)} required />
      </label>
      <label>
        Capacity
        <input
          type="number"
          min="1"
          value={form.capacity}
          onChange={(event) => update('capacity', event.target.value)}
          required
        />
      </label>
      <div className="editor-actions">
        <button type="submit" className="primary-button">
          {location ? 'Save location' : 'Add location'}
        </button>
        {onCancel && (
          <button type="button" className="secondary-button" onClick={onCancel}>
            Cancel
          </button>
        )}
      </div>
    </form>
  );
}

const App = () => {
  const [filters, setFilters] = useState(initialFilters);
  const [products, setProducts] = useState([]);
  const [locations, setLocations] = useState([]);
  const [status, setStatus] = useState('loading');
  const [error, setError] = useState('');
  const [expandedProductId, setExpandedProductId] = useState(null);
  const [inventoryEditor, setInventoryEditor] = useState(null);
  const [locationEditor, setLocationEditor] = useState(null);
  const [showLocationForm, setShowLocationForm] = useState(false);
  const [actionError, setActionError] = useState('');

  useEffect(() => {
    const controller = new globalThis.AbortController();
    const timeout = globalThis.setTimeout(async () => {
      setStatus('loading');
      setError('');

      try {
        const result = await getProducts(filters, controller.signal);
        setProducts(Array.isArray(result) ? result : []);
        setStatus('ready');
      } catch (requestError) {
        if (requestError.name !== 'AbortError') {
          setError(requestError.message);
          setStatus('error');
        }
      }
    }, 250);

    return () => {
      globalThis.clearTimeout(timeout);
      controller.abort();
    };
  }, [filters]);

  useEffect(() => {
    const controller = new globalThis.AbortController();
    getLocations(controller.signal)
      .then((result) => {
        setLocations(
          Array.isArray(result) ? result.filter((location) => location && typeof location.capacity === 'number') : [],
        );
      })
      .catch((requestError) => {
        if (requestError.name !== 'AbortError') setActionError(requestError.message);
      });
    return () => controller.abort();
  }, []);

  const reloadData = async () => {
    const [productsResult, locationsResult] = await Promise.all([getProducts(filters), getLocations()]);
    setProducts(Array.isArray(productsResult) ? productsResult : []);
    setLocations(
      Array.isArray(locationsResult)
        ? locationsResult.filter((location) => location && typeof location.capacity === 'number')
        : [],
    );
  };

  const runAction = async (action) => {
    setActionError('');
    try {
      await action();
      await reloadData();
    } catch (requestError) {
      setActionError(requestError.message);
    }
  };

  const updateFilter = (name, value) => {
    setFilters((currentFilters) => ({ ...currentFilters, [name]: value }));
  };

  const saveInventory = (productId, inventoryId, inventory) =>
    runAction(async () => {
      if (inventoryId) await updateInventory(inventoryId, inventory);
      else await addInventory(productId, inventory);
      setInventoryEditor(null);
    });

  const saveLocation = (locationId, location) =>
    runAction(async () => {
      if (locationId) await updateLocation(locationId, location);
      else await addLocation(location);
      setLocationEditor(null);
      setShowLocationForm(false);
    });

  return (
    <main className="page-shell">
      <header className="page-header">
        <p className="eyebrow">Inventory</p>
        <h1>Products</h1>
        <p className="intro">Browse active products, manage stock across locations, and monitor available capacity.</p>
      </header>

      {actionError && (
        <div className="action-error" role="status">
          {actionError}
          <button type="button" onClick={() => setActionError('')} aria-label="Dismiss error">
            ×
          </button>
        </div>
      )}

      <section className="catalogue-card locations-card" aria-labelledby="locations-heading">
        <div className="section-heading">
          <div>
            <p className="eyebrow">Capacity overview</p>
            <h2 id="locations-heading">Locations</h2>
          </div>
          <button
            type="button"
            className="primary-button"
            onClick={() => {
              setLocationEditor(null);
              setShowLocationForm(true);
            }}
          >
            Add location
          </button>
        </div>

        {showLocationForm && !locationEditor && (
          <LocationForm onCancel={() => setShowLocationForm(false)} onSubmit={(value) => saveLocation(null, value)} />
        )}

        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th scope="col">Location</th>
                <th scope="col">Region</th>
                <th scope="col">Country</th>
                <th scope="col">Capacity</th>
                <th scope="col">Used</th>
                <th scope="col">Remaining</th>
                <th scope="col">Actions</th>
              </tr>
            </thead>
            <tbody>
              {locations.map((location) => (
                <React.Fragment key={location.id}>
                  <tr>
                    <th scope="row">{location.name}</th>
                    <td>{location.region}</td>
                    <td>{location.country}</td>
                    <td>{location.capacity}</td>
                    <td>{location.usedCapacity}</td>
                    <td className={location.remainingCapacity < 0 ? 'capacity-warning' : ''}>{location.remainingCapacity}</td>
                    <td className="inline-actions">
                      <button
                        type="button"
                        className="text-button"
                        onClick={() => {
                          setLocationEditor(location);
                          setShowLocationForm(false);
                        }}
                      >
                        Edit
                      </button>
                      <button
                        type="button"
                        className="danger-button"
                        onClick={() => {
                          if (globalThis.confirm(`Remove ${location.name}?`)) {
                            runAction(() => deleteLocation(location.id));
                          }
                        }}
                      >
                        Remove
                      </button>
                    </td>
                  </tr>
                  {locationEditor?.id === location.id && (
                    <tr className="editor-row">
                      <td colSpan="7">
                        <LocationForm
                          location={location}
                          onCancel={() => setLocationEditor(null)}
                          onSubmit={(value) => saveLocation(location.id, value)}
                        />
                      </td>
                    </tr>
                  )}
                </React.Fragment>
              ))}
            </tbody>
          </table>
        </div>
      </section>

      <section className="catalogue-card" aria-labelledby="catalogue-heading">
        <div className="toolbar">
          <div>
            <h2 id="catalogue-heading">Product catalogue</h2>
            <p className="result-count" aria-live="polite">
              {status === 'ready' ? `${products.length} ${products.length === 1 ? 'product' : 'products'}` : 'Updating results'}
            </p>
          </div>
          <div className="filters" role="search">
            <label className="search-field">
              <span>Search products</span>
              <input
                type="search"
                value={filters.search}
                onChange={(event) => updateFilter('search', event.target.value)}
                placeholder="Search by name or SKU"
              />
            </label>
            <label className="category-field">
              <span>Category</span>
              <select value={filters.category} onChange={(event) => updateFilter('category', event.target.value)}>
                <option value="">All categories</option>
                {categories.map((category) => (
                  <option key={category} value={category}>
                    {category}
                  </option>
                ))}
              </select>
            </label>
          </div>
        </div>

        {status === 'loading' && <p className="state-message">Loading products...</p>}
        {status === 'error' && (
          <div className="state-message error-message" role="alert">
            <strong>Products could not be loaded.</strong>
            <span>{error}</span>
          </div>
        )}
        {status === 'ready' && products.length === 0 && <p className="state-message">No products match the current filters.</p>}
        {status === 'ready' && products.length > 0 && (
          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th scope="col">Product</th>
                  <th scope="col">SKU</th>
                  <th scope="col">Category</th>
                  <th scope="col">Status</th>
                  <th scope="col">Locations</th>
                  <th scope="col">Used capacity</th>
                  <th scope="col">Price</th>
                  <th scope="col">Margin</th>
                </tr>
              </thead>
              <tbody>
                {products.map((product) => {
                  const inventory = product.inventory ?? [];
                  const usedCapacity = inventory.reduce((total, item) => total + item.amount * item.capacityCost, 0);
                  const price = Number(product.price || 0);
                  const margin = Number(product.margin || 0);
                  const expanded = expandedProductId === product.id;

                  return (
                    <React.Fragment key={product.id}>
                      <tr className={expanded ? 'product-row expanded' : 'product-row'}>
                        <th scope="row">
                          <button
                            type="button"
                            className="product-toggle"
                            onClick={() => setExpandedProductId(expanded ? null : product.id)}
                            aria-expanded={expanded}
                          >
                            <span className="toggle-icon">{expanded ? '−' : '+'}</span>
                            {product.name}
                          </button>
                        </th>
                        <td>{product.sku}</td>
                        <td>{product.category?.name || 'Uncategorised'}</td>
                        <td>
                          <button className={product.isActive ? 'status-pill active' : 'status-pill inactive'}
                            onClick={() => toggleProductActive(product.id)}>
                            {product.isActive ? 'Active' : 'Inactive'}</button>
                        </td>
                        <td>{inventory.length || 'Not set'}</td>
                        <td>{usedCapacity || 'Not set'}</td>
                        <td>{price ? currency.format(price) : 'Not set'}</td>
                        <td className={margin >= 0 ? 'margin-positive' : 'margin-negative'}>
                          {price || margin ? `${margin >= 0 ? '+' : ''}${currency.format(margin)}` : 'Not set'}
                        </td>
                      </tr>
                      {expanded && (
                        <tr className="detail-row">
                          <td colSpan="8">
                            <div className="product-details">
                              <div className="detail-heading">
                                <div>
                                  <p className="detail-kicker">Stock placement</p>
                                  <h3>{product.name}</h3>
                                </div>
                                <button
                                  type="button"
                                  className="secondary-button"
                                  onClick={() => setInventoryEditor({ productId: product.id, inventoryId: null })}
                                >
                                  Add location
                                </button>
                              </div>

                              {inventory.length > 0 ? (
                                <div className="inventory-list">
                                  {inventory.map((item) => (
                                    <div className="inventory-item" key={item.id}>
                                      {inventoryEditor?.inventoryId === item.id ? (
                                        <InventoryForm
                                          key={`${item.id}-editor`}
                                          inventory={item}
                                          locations={locations}
                                          onCancel={() => setInventoryEditor(null)}
                                          onSubmit={(value) => saveInventory(product.id, item.id, value)}
                                        />
                                      ) : (
                                        <>
                                          <div>
                                            <strong>{item.location?.name || 'Unknown location'}</strong>
                                            <span>
                                              {item.location?.region}, {item.location?.country}
                                            </span>
                                          </div>
                                          <div className="inventory-metric">
                                            <span>Amount</span>
                                            <strong>{item.amount}</strong>
                                          </div>
                                          <div className="inventory-metric">
                                            <span>Used capacity</span>
                                            <strong>{item.amount * item.capacityCost}</strong>
                                          </div>
                                          <div className="inline-actions">
                                            <button
                                              type="button"
                                              className="text-button"
                                              onClick={() => setInventoryEditor({ productId: product.id, inventoryId: item.id })}
                                            >
                                              Edit
                                            </button>
                                            <button
                                              type="button"
                                              className="danger-button"
                                              onClick={() => {
                                                if (globalThis.confirm('Remove this inventory record?')) {
                                                  runAction(() => deleteInventory(item.id));
                                                }
                                              }}
                                            >
                                              Remove
                                            </button>
                                          </div>
                                        </>
                                      )}
                                    </div>
                                  ))}
                                </div>
                              ) : (
                                <p className="empty-detail">This product is not assigned to a location.</p>
                              )}

                              {inventoryEditor?.productId === product.id && inventoryEditor.inventoryId === null && (
                                <InventoryForm
                                  key={`${product.id}-new`}
                                  locations={locations}
                                  onCancel={() => setInventoryEditor(null)}
                                  onSubmit={(value) => saveInventory(product.id, null, value)}
                                />
                              )}
                            </div>
                          </td>
                        </tr>
                      )}
                    </React.Fragment>
                  );
                })}
              </tbody>
            </table>
          </div>
        )}
      </section>

    </main>
  );
};

export default App;
