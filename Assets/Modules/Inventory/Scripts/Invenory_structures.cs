using System;

namespace Modules.Inventories
{
    public partial class Inventory
    { 
        private struct ItemInfo
        {
            public int X;
            public int Y;
            public int Index;

            public ItemInfo(int x, int y, int index)
            {
                X = x;
                Y = y;
                Index = index;
            }
        }

        private struct MapEntry
        {
            public int Hash;
            public Item Key;
            public ItemInfo Value;
        }
        
        private struct ItemMap
        {
            private const int LoadFactorNumerator = 7;
            private const int LoadFactorDenominator = 10;

            private MapEntry[] _entries;
            private int _count;
            private int _used;
            
            public int Count => _count;

            public ItemMap(int capacity)
            {
                int cap = 1;
                while (cap < capacity)
                    cap <<= 1;
                
                _entries = new MapEntry[cap];
                _count = 0;
                _used = 0;
            }

            public ItemMap Clone()
            {
                ItemMap clone = default;
                if (_entries == null)
                    return clone;

                clone._entries = (MapEntry[])_entries.Clone();
                clone._count = _count;
                clone._used = _used;
                return clone;
            }

            public void Clear()
            {
                if (_entries == null || _count == 0)
                    return;

                Array.Clear(_entries, 0, _entries.Length);
                _count = 0;
                _used = 0;
            }

            private static int KeyHash(Item key)
            {
                int h = key.GetHashCode() & 0x7FFFFFFF;
                return h == 0 ? 1 : h;
            }

            public bool Contains(Item key) => TryGet(key, out _);

            public bool TryGet(Item key, out ItemInfo value)
            {
                if (key == null || _entries == null)
                {
                    value = default;
                    return false;
                }

                int hash = KeyHash(key);
                int index = FindIndex(key, hash);
                if (index < 0)
                {
                    value = default;
                    return false;
                }

                value = _entries[index].Value;
                return true;
            }

            public bool TryAdd(Item key, ItemInfo value)
            {
                if (key == null)
                    return false;

                EnsureCapacity();
                return TryInsert(key, value, overwrite: false);
            }

            public bool TryUpdate(Item key, ItemInfo value)
            {
                if (key == null || _entries == null)
                    return false;

                int hash = KeyHash(key);
                int index = FindIndex(key, hash);
                if (index < 0)
                    return false;

                _entries[index].Value = value;
                return true;
            }

            public bool Remove(Item key)
            {
                if (key == null || _entries == null)
                    return false;

                int hash = KeyHash(key);
                int index = FindIndex(key, hash);
                if (index < 0)
                    return false;

                _count--;
                if (_count == 0)
                {
                    Array.Clear(_entries, 0, _entries.Length);
                    _used = 0;
                }
                else
                {
                    _entries[index].Hash = -1;
                    _entries[index].Key = null;
                    _entries[index].Value = default;
                }

                return true;
            }

            private int FindIndex(Item key, int hash)
            {
                int mask = _entries.Length - 1;
                int index = hash & mask;

                while (true)
                {
                    int entryHash = _entries[index].Hash;
                    if (entryHash == 0)
                        return -1;

                    if (entryHash == hash && _entries[index].Key.Equals(key))
                        return index;

                    index = (index + 1) & mask;
                }
            }

            private void EnsureCapacity()
            {
                if (_entries == null || _entries.Length == 0)
                {
                    _entries = new MapEntry[4];
                    return;
                }

                if ((_used + 1) * LoadFactorDenominator >= _entries.Length * LoadFactorNumerator)
                    Resize(_entries.Length * 2);
            }

            private void Resize(int capacity)
            {
                int cap = 1;
                while (cap < capacity)
                    cap <<= 1;

                MapEntry[] oldEntries = _entries;
                _entries = new MapEntry[cap];
                _count = 0;
                _used = 0;

                for (int i = 0; i < oldEntries.Length; i++)
                {
                    int hash = oldEntries[i].Hash;
                    if (hash > 0)
                        InsertRehash(oldEntries[i].Key, hash, oldEntries[i].Value);
                }
            }

            private void InsertRehash(Item key, int hash, ItemInfo value)
            {
                int mask = _entries.Length - 1;
                int index = hash & mask;

                while (_entries[index].Hash != 0)
                    index = (index + 1) & mask;

                _entries[index].Hash = hash;
                _entries[index].Key = key;
                _entries[index].Value = value;
                _count++;
                _used++;
            }

            private bool TryInsert(Item key, ItemInfo value, bool overwrite)
            {
                int hash = KeyHash(key);
                int mask = _entries.Length - 1;
                int index = hash & mask;
                int firstRemoved = -1;

                while (true)
                {
                    int entryHash = _entries[index].Hash;
                    if (entryHash == 0)
                    {
                        int targetIndex = firstRemoved >= 0 ? firstRemoved : index;
                        _entries[targetIndex].Hash = hash;
                        _entries[targetIndex].Key = key;
                        _entries[targetIndex].Value = value;
                        _count++;
                        if (firstRemoved < 0)
                            _used++;
                        return true;
                    }

                    if (entryHash == hash && _entries[index].Key.Equals(key))
                    {
                        if (overwrite)
                        {
                            _entries[index].Value = value;
                            return true;
                        }

                        return false;
                    }

                    if (entryHash < 0 && firstRemoved < 0)
                        firstRemoved = index;

                    index = (index + 1) & mask;
                }
            }
        }
    }
}
