import { useState, useEffect, useCallback } from 'react'
import { api, ShoppingItem } from './api'

const NAMES = ['Rasmus', 'Kathrine', 'Claire', 'Katja'];

export default function App() {
  const [items, setItems] = useState<ShoppingItem[]>([]);
  const [name, setName] = useState('');
  const [quantity, setQuantity] = useState('');
  const [addedBy, setAddedBy] = useState(() => localStorage.getItem('addedBy') || NAMES[0]);

  const refresh = useCallback(() => {
    api.getItems().then(setItems).catch(() => {});
  }, []);

  useEffect(() => { refresh(); }, [refresh]);
  useEffect(() => {
    const id = setInterval(refresh, 30000);
    return () => clearInterval(id);
  }, [refresh]);

  useEffect(() => { localStorage.setItem('addedBy', addedBy); }, [addedBy]);

  const handleAdd = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) return;
    await api.addItem(name.trim(), addedBy, quantity.trim() || undefined);
    setName('');
    setQuantity('');
    refresh();
  };

  const unchecked = items.filter(i => !i.isChecked);
  const checked = items.filter(i => i.isChecked);

  return (
    <div className="min-h-screen bg-stone-50 text-stone-800">
      <div className="max-w-lg mx-auto px-4 py-6">
        <h1 className="text-2xl font-light tracking-wide mb-6">Indkøbsliste</h1>

        <form onSubmit={handleAdd} className="mb-6 space-y-2">
          <div className="flex gap-2">
            <input
              value={name}
              onChange={e => setName(e.target.value)}
              placeholder="Tilføj vare..."
              className="flex-1 px-3 py-2 bg-white border border-stone-200 rounded-lg text-sm focus:outline-none focus:border-stone-400"
            />
            <input
              value={quantity}
              onChange={e => setQuantity(e.target.value)}
              placeholder="Antal"
              className="w-20 px-3 py-2 bg-white border border-stone-200 rounded-lg text-sm focus:outline-none focus:border-stone-400"
            />
            <button type="submit" className="px-4 py-2 bg-stone-800 text-white rounded-lg text-sm hover:bg-stone-700">
              +
            </button>
          </div>
          <select
            value={addedBy}
            onChange={e => setAddedBy(e.target.value)}
            className="px-2 py-1 text-xs bg-white border border-stone-200 rounded"
          >
            {NAMES.map(n => <option key={n} value={n}>{n}</option>)}
          </select>
        </form>

        {unchecked.length === 0 && checked.length === 0 && (
          <p className="text-stone-400 text-sm text-center py-8">Listen er tom</p>
        )}

        <ul className="space-y-1">
          {unchecked.map(item => (
            <li key={item.id} className="flex items-center gap-3 px-3 py-2 bg-white rounded-lg border border-stone-100">
              <input
                type="checkbox"
                checked={false}
                onChange={() => { api.toggleItem(item.id).then(refresh); }}
                className="w-4 h-4 accent-stone-600"
              />
              <div className="flex-1 min-w-0">
                <span className="text-sm">{item.name}</span>
                {item.quantity && <span className="text-xs text-stone-400 ml-2">{item.quantity}</span>}
                <span className="block text-[10px] text-stone-400">{item.addedBy}</span>
              </div>
              <button onClick={() => { api.deleteItem(item.id).then(refresh); }} className="text-stone-300 hover:text-red-400 text-lg">×</button>
            </li>
          ))}
        </ul>

        {checked.length > 0 && (
          <>
            <div className="flex items-center justify-between mt-6 mb-2">
              <span className="text-xs text-stone-400 uppercase tracking-wider">Afkrydsede</span>
              <button
                onClick={() => { api.clearChecked().then(refresh); }}
                className="text-xs text-stone-400 hover:text-red-400"
              >
                Ryd afkrydsede
              </button>
            </div>
            <ul className="space-y-1">
              {checked.map(item => (
                <li key={item.id} className="flex items-center gap-3 px-3 py-2 bg-stone-100 rounded-lg">
                  <input
                    type="checkbox"
                    checked={true}
                    onChange={() => { api.toggleItem(item.id).then(refresh); }}
                    className="w-4 h-4 accent-stone-400"
                  />
                  <div className="flex-1 min-w-0">
                    <span className="text-sm line-through text-stone-400">{item.name}</span>
                    {item.quantity && <span className="text-xs text-stone-300 ml-2">{item.quantity}</span>}
                    <span className="block text-[10px] text-stone-300">{item.addedBy}</span>
                  </div>
                  <button onClick={() => { api.deleteItem(item.id).then(refresh); }} className="text-stone-300 hover:text-red-400 text-lg">×</button>
                </li>
              ))}
            </ul>
          </>
        )}
      </div>
    </div>
  );
}
