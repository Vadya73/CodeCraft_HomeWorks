using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Modules.Inventories
{
    public struct InventorySlotData
    {
        public int XPosition;
        public int YPosition;
        public Item Item;
    }
    /// <summary>
    /// Обработку на ошибки в основном подсказывал копилот (ArgumentException), не пугайся что там такой текст официальный XD
    /// если честно я ненавижу алгоритмы и литкод, а тут мне пришлось прям напрячься чтоб представить выдуманный инвентарь
    /// так что я надеюсь мои подходы к решению задачи не стали костыльными и странными, я не силён в таких решениях
    /// </summary>
    public class Inventory : IEnumerable<Item>
    {
        public event Action<Item, Vector2Int> OnAdded;
        public event Action<Item, Vector2Int> OnRemoved;
        public event Action<Item, Vector2Int> OnMoved;
        public event Action OnCleared;

        private InventorySlotData[] _slots;
        private int _width;
        private int _height;

        private Item[] _items;
        private int _itemsCount;

        public int Width => _width;
        public int Height => _height;
        public int Count => _itemsCount;
        public InventorySlotData[] Slots => _slots;

        public Inventory(int width, int height)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");

            Initialize(width, height);
        }

        public Inventory(int width, int height, params KeyValuePair<Item, Vector2Int>[] items)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Initialize(width, height);

            for (int i = 0; i < items.Length; i++)
                AddItem(items[i].Key, items[i].Value);
        }

        public Inventory(int width, int height, params Item[] items)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Initialize(width, height);

            for (int i = 0; i < items.Length; i++)
                AddItem(items[i]);
        }

        public Inventory(int width, int height, IEnumerable<KeyValuePair<Item, Vector2Int>> items)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Initialize(width, height);

            foreach (var kv in items)
                AddItem(kv.Key, kv.Value);
        }

        public Inventory(int width, int height, IEnumerable<Item> items)
        {
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Dimensions must be positive");
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            Initialize(width, height);

            foreach (var it in items)
                AddItem(it);
        }

        /// <summary>
        /// Creates new inventory 
        /// </summary>
        public Inventory(Inventory inventory)
        {
            Clone(inventory);
        }
        
        /// <summary>
        /// Checks for adding an item on a specified position
        /// </summary>
        public bool CanAddItem(Item item, Vector2Int position)
        {
            return CanPlaceItem(item, position, allowAlreadyInInventory: false);
        }

        public bool CanAddItem(Item item, int startX, int startY)
        {
            return CanAddItem(item, new Vector2Int(startX, startY));
        }

        /// <summary>
        /// Adds an item on a specified position
        /// </summary>
        public bool AddItem(Item item, Vector2Int position)
        {
            if (!CanAddItem(item, position))
                return false;

            PlaceItem(item, position);
            TrackAddedUniqueItem(item);

            OnAdded?.Invoke(item, position);
            return true;
        }

        public bool AddItem(Item item, int startX, int startY)
        {
            return AddItem(item, new Vector2Int(startX, startY));
        }

        /// <summary>
        /// Checks for adding an item on a free position
        /// </summary>
        public bool CanAddItem(Item item)
        {
            if (item == null)
                return false;

            ValidateItemSize(item);

            if (Contains(item))
                return false;

            return FindFreePosition(item, out _);
        }

        /// <summary>
        /// Adds an item on a free position
        /// </summary>
        public bool AddItem(Item item)
        {
            if (item == null)
                return false;

            ValidateItemSize(item);

            if (Contains(item))
                return false;

            if (FindFreePosition(item, out var position))
                return AddItem(item, position);

            return false;
        }

        /// <summary>
        /// Returns a free position for a specified item
        /// </summary>
        public bool FindFreePosition(Item item, out Vector2Int position)
        {
            if (item == null)
            {
                position = default;
                return false;
            }

            return FindFreePosition(item.Size, out position);
        }

        public bool FindFreePosition(Vector2Int size, out Vector2Int position)
        {
            return FindFreePosition(size.x, size.y, out position);
        }

        public bool FindFreePosition(int sizeX, int sizeY, out Vector2Int position)
        {
            if (sizeX <= 0 || sizeY <= 0)
                throw new ArgumentException("Size must be positive");

            for (int y = 0; y <= _height - sizeY; y++)
            {
                for (int x = 0; x <= _width - sizeX; x++)
                {
                    if (IsFreeSpace(x, y, x + sizeX, y + sizeY))
                    {
                        position = new Vector2Int(x, y);
                        return true;
                    }
                }
            }

            position = default;
            return false;
        }
        
        /// <summary>
        /// Checks if the specified element exists
        /// </summary>
        public bool Contains(Item item)
        {
            if (item == null)
                return false;

            return IndexOfItem(item) >= 0;
        }
        
        /// <summary>
        /// Checks if the specified position is occupied
        /// </summary>
        public bool IsOccupied(Vector2Int position) => IsOccupied(position.x, position.y);

        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return true;

            return _slots[y * _width + x].Item != null;
        }

        /// <summary>
        /// Checks if the specified position is free
        /// </summary>
        public bool IsFree(Vector2Int position) => IsFree(position.x, position.y);

        public bool IsFree(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            return _slots[y * _width + x].Item == null;
        }

        /// <summary>
        /// Removes specified item
        /// </summary>
        public bool RemoveItem(Item item)
        {
            if (item == null)
                return false;

            int itemIndex = IndexOfItem(item);
            if (itemIndex < 0)
                return false;

            bool hasItem = false;
            Vector2Int position = Vector2Int.zero;

            for (int i = 0; i < _slots.Length; i++)
            {
                var it = _slots[i].Item;
                if (it != null && it.Equals(item))
                {
                    if (!hasItem)
                    {
                        position = new Vector2Int(_slots[i].XPosition, _slots[i].YPosition);
                        hasItem = true;
                    }
                    _slots[i].Item = null;
                }
            }

            if (hasItem)
            {
                RemoveUniqueItemAt(itemIndex);
                OnRemoved?.Invoke(item, position);
                return true;
            }

            return false;
        }

        public bool RemoveItem(Item item, out Vector2Int position)
        {
            if (item == null)
            {
                position = default;
                return false;
            }

            int itemIndex = IndexOfItem(item);
            if (itemIndex < 0)
            {
                position = default;
                return false;
            }

            bool hasItem = false;
            position = default;

            for (int i = 0; i < _slots.Length; i++)
            {
                var it = _slots[i].Item;
                if (it != null && it.Equals(item))
                {
                    if (!hasItem)
                    {
                        position = new Vector2Int(_slots[i].XPosition, _slots[i].YPosition);
                        hasItem = true;
                    }
                    _slots[i].Item = null;
                }
            }

            if (hasItem)
            {
                RemoveUniqueItemAt(itemIndex);
                OnRemoved?.Invoke(item, position);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns an item at specified position 
        /// </summary>
        public Item GetItem(Vector2Int position)
        {
            return GetItem(position.x, position.y);
        }

        public Item GetItem(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                throw new IndexOutOfRangeException("Position out of range");

            return _slots[y * _width + x].Item;
        }

        public bool TryGetItem(Vector2Int position, out Item item) => TryGetItem(position.x, position.y, out item);

        public bool TryGetItem(int x, int y, out Item item)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
            {
                item = null;
                return false;
            }

            item = _slots[y * _width + x].Item;
            return item != null;
        }

        /// <summary>
        /// Returns positions of a specified item 
        /// </summary>
        public Vector2Int[] GetPositions(Item item)
        {
            if (item == null)
                throw new NullReferenceException(nameof(item));

            if (!TryGetPositions(item, out var positions))
                throw new KeyNotFoundException("Item not found");

            return positions;
        }

        public bool TryGetPositions(Item item, out Vector2Int[] positions)
        {
            if (item == null)
            {
                positions = null;
                return false;
            }

            int count = 0;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int index = y * _width + x;
                    var it = _slots[index].Item;
                    if (it != null && it.Equals(item))
                        count++;
                }
            }

            if (count == 0)
            {
                positions = null;
                return false;
            }

            positions = new Vector2Int[count];
            int p = 0;
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int index = y * _width + x;
                    var it = _slots[index].Item;
                    
                    if (it != null && it.Equals(item))
                        positions[p++] = new Vector2Int(x, y);
                }
            }

            return true;
        }

        /// <summary>
        /// Clears all items 
        /// </summary>
        public void Clear()
        {
            if (_itemsCount == 0)
                return;

            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Item = null;

            for (int i = 0; i < _itemsCount; i++)
                _items[i] = null;

            _itemsCount = 0;

            OnCleared?.Invoke();
        }

        /// <summary>
        /// Returns count of items with a specified name
        /// </summary>
        public int GetItemCount(string name)
        {
            int c = 0;
            for (int i = 0; i < _itemsCount; i++)
            {
                if (_items[i] != null && _items[i].Name == name)
                    c++;
            }
            return c;
        }

        public bool MoveItem(Item item, Vector2Int position)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            int itemIndex = IndexOfItem(item);
            
            if (itemIndex < 0)
                return false;

            int expectedCells = item.Size.x * item.Size.y;
            
            if (expectedCells <= 0)
                throw new ArgumentException("Item size must be positive");

            int[] removedIndices = new int[expectedCells];
            int removedCount = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                var it = _slots[i].Item;
                
                if (it != null && it.Equals(item))
                {
                    _slots[i].Item = null;
                    
                    if (removedCount < removedIndices.Length)
                        removedIndices[removedCount++] = i;
                    else
                    {
                        Array.Resize(ref removedIndices, removedIndices.Length * 2);
                        removedIndices[removedCount++] = i;
                    }
                }
            }

            if (removedCount == 0)
                return false;

            if (CanPlaceItem(item, position, allowAlreadyInInventory: true))
            {
                PlaceItem(item, position);
                OnMoved?.Invoke(item, position);
                return true;
            }

            for (int i = 0; i < removedCount; i++)
                _slots[removedIndices[i]].Item = item;

            return false;
        }

        /// <summary>
        /// Rearranges an inventory space with max free slots 
        /// </summary>
        public void OptimizeSpace()
        {
            if (_itemsCount <= 1)
                return;

            Item[] temp = new Item[_itemsCount];
            for (int i = 0; i < _itemsCount; i++)
                temp[i] = _items[i];

            QuickSortItems(temp, 0, temp.Length - 1);

            Clear();

            for (int i = 0; i < temp.Length; i++)
                AddItem(temp[i]);
        }
        
        /// <summary>
        /// Iterates by all items 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<Item> GetEnumerator()
        {
            for (int i = 0; i < _itemsCount; i++)
            {
                if (_items[i] != null)
                    yield return _items[i];
            }
        }

        /// <summary>
        /// Copies items to a specified matrix
        /// </summary>
        public void CopyTo(Item[,] matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException(nameof(matrix));

            int matrixWidth = matrix.GetLength(0);
            int matrixHeight = matrix.GetLength(1);

            int copyWidth = Math.Min(_width, matrixWidth);
            int copyHeight = Math.Min(_height, matrixHeight);

            for (int y = 0; y < copyHeight; y++)
            {
                int row = y * _width;
                
                for (int x = 0; x < copyWidth; x++)
                    matrix[x, y] = _slots[row + x].Item;
            }
        }

        /// <summary>
        /// Returns an inventory matrix in string format
        /// </summary>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var item = GetItem(x, y);
                    sb.Append(item != null ? "X" : ".");
                }

                if (y < _height - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
        
        private void ValidateItemSize(Item item)
        {
            if (item.Size.x <= 0 || item.Size.y <= 0)
                throw new ArgumentException("Item size must be positive");
        }
        
        private void Clone(Inventory inventory)
        {
            if (inventory == null)
                throw new ArgumentNullException(nameof(inventory));

            _width = inventory._width;
            _height = inventory._height;

            _slots = new InventorySlotData[inventory._slots.Length];
            Array.Copy(inventory._slots, _slots, inventory._slots.Length);

            _itemsCount = inventory._itemsCount;
            _items = new Item[inventory._items != null ? inventory._items.Length : Math.Max(4, _itemsCount)];
            
            if (_itemsCount > 0)
                Array.Copy(inventory._items, _items, _itemsCount);
        }

        private void Initialize(int width, int height)
        {
            _width = width;
            _height = height;

            int inventorySize = width * height;
            _slots = new InventorySlotData[inventorySize];

            int x = 0;
            int y = 0;

            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].XPosition = x;
                _slots[i].YPosition = y;
                _slots[i].Item = null;

                x++;
                if (x >= width)
                {
                    x = 0;
                    y++;
                }
            }

            _items = new Item[Math.Max(4, Math.Min(16, inventorySize))];
            _itemsCount = 0;
        }
        
        private bool IsFreeSpace(int startX, int startY, int endX, int endY)
        {
            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (IsOccupied(x, y))
                        return false;
                }
            }

            return true;
        }

        private bool CanPlaceItem(Item item, Vector2Int position, bool allowAlreadyInInventory)
        {
            if (item == null)
                return false;

            ValidateItemSize(item);

            if (!allowAlreadyInInventory && Contains(item))
                return false;

            int startX = position.x;
            int startY = position.y;
            int endX = startX + item.Size.x;
            int endY = startY + item.Size.y;

            if (startX < 0 || startY < 0 || endX > _width || endY > _height)
                return false;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    if (IsOccupied(x, y))
                        return false;
                }
            }

            return true;
        }

        private void PlaceItem(Item item, Vector2Int position)
        {
            int startX = position.x;
            int startY = position.y;
            int endX = startX + item.Size.x;
            int endY = startY + item.Size.y;

            for (int y = startY; y < endY; y++)
            {
                int row = y * _width;
                
                for (int x = startX; x < endX; x++)
                    _slots[row + x].Item = item;
            }
        }
        
        private int IndexOfItem(Item item)
        {
            for (int i = 0; i < _itemsCount; i++)
            {
                if (_items[i] != null && _items[i].Equals(item))
                    return i;
            }
            return -1;
        }

        private void TrackAddedUniqueItem(Item item)
        {
            EnsureItemsCapacity(_itemsCount + 1);
            _items[_itemsCount++] = item;
        }

        private void EnsureItemsCapacity(int required)
        {
            if (_items == null)
            {
                _items = new Item[Math.Max(4, required)];
                return;
            }

            if (required <= _items.Length)
                return;

            int newCap = _items.Length * 2;
            if (newCap < required) newCap = required;
            Array.Resize(ref _items, newCap);
        }

        private void RemoveUniqueItemAt(int index)
        {
            int last = _itemsCount - 1;
            _items[index] = _items[last];
            _items[last] = null;
            _itemsCount--;
        }
        
        private void QuickSortItems(Item[] arr, int left, int right)
        {
            int i = left;
            int j = right;
            Item pivot = arr[left + ((right - left) >> 1)];

            while (i <= j)
            {
                while (CompareItems(arr[i], pivot) < 0) 
                    i++;
                
                while (CompareItems(arr[j], pivot) > 0) 
                    j--;

                if (i <= j)
                {
                    (arr[i], arr[j]) = (arr[j], arr[i]);
                    i++;
                    j--;
                }
            }

            if (left < j) 
                QuickSortItems(arr, left, j);
            if (i < right) 
                QuickSortItems(arr, i, right);
        }
        
        private int CompareItems(Item a, Item b)
        {
            int areaA = a.Size.x * a.Size.y;
            int areaB = b.Size.x * b.Size.y;

            if (areaA != areaB)
                return areaB.CompareTo(areaA);

            return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
        }
    }
}