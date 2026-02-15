const BASE = import.meta.env.VITE_API_URL || '';

export interface ShoppingItem {
  id: number;
  name: string;
  quantity?: string | null;
  isChecked: boolean;
  addedBy: string;
  createdAt: string;
}

export const api = {
  getItems: (): Promise<ShoppingItem[]> =>
    fetch(`${BASE}/api/items`).then(r => r.json()),

  addItem: (name: string, addedBy: string, quantity?: string): Promise<ShoppingItem> =>
    fetch(`${BASE}/api/items`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, addedBy, quantity: quantity || null }),
    }).then(r => r.json()),

  toggleItem: (id: number): Promise<ShoppingItem> =>
    fetch(`${BASE}/api/items/${id}/toggle`, { method: 'PUT' }).then(r => r.json()),

  deleteItem: (id: number): Promise<void> =>
    fetch(`${BASE}/api/items/${id}`, { method: 'DELETE' }).then(() => {}),

  clearChecked: (): Promise<void> =>
    fetch(`${BASE}/api/items/checked`, { method: 'DELETE' }).then(() => {}),
};
