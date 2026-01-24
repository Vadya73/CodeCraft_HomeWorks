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
    public class Inventory : IEnumerable<Item>
    {
        public event Action<Item, Vector2Int> OnAdded;
        public event Action<Item, Vector2Int> OnRemoved;
        public event Action<Item, Vector2Int> OnMoved;
        public event Action OnCleared;

        private InventorySlotData[] _slots;
        private int _width;
        private int _height;
        
        public int Width => _width;
        public int Height => _height;
        public int Count
        {
            get
            {
                var uniqueItems = new HashSet<Item>();
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (_slots[i].Item != null)
                        uniqueItems.Add(_slots[i].Item);
                }
                return uniqueItems.Count;
            }
        }
        public InventorySlotData[] Slots => _slots;

        public Inventory(int width, int height)
        {
            if (width <= 0 || height <= 0) 
                throw new ArgumentException("Dimensions must be positive");
            
            InitializeInventorySlots(width, height);
        }

        public Inventory(int width, int height, params KeyValuePair<Item, Vector2Int>[] items)
        {
            if (width <= 0 || height <= 0) 
                throw new ArgumentException("Dimensions must be positive");
            
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            
            InitializeInventorySlots(width, height);
            
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                AddItem(item.Key, item.Value);
            }
        }

        public Inventory(int width, int height, params Item[] items)
        {
            if (width <= 0 || height <= 0) 
                throw new ArgumentException("Dimensions must be positive");
            
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            
            InitializeInventorySlots(width, height);
            
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                AddItem(item);
            }
        }

        public Inventory(int width, int height, IEnumerable<KeyValuePair<Item, Vector2Int>> items)
        {
            if (width <= 0 || height <= 0) 
                throw new ArgumentException("Dimensions must be positive");
            
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            
            InitializeInventorySlots(width, height);
            
            foreach (var item in items)
                AddItem(item.Key, item.Value);
        }

        public Inventory(int width, int height, IEnumerable<Item> items)
        {
            if (width <= 0 || height <= 0) 
                throw new ArgumentException("Dimensions must be positive");
            
            if (items == null) 
                throw new ArgumentNullException(nameof(items));
            
            InitializeInventorySlots(width, height);
            
            foreach (var item in items)
                AddItem(item);
        }

        /// <summary>
        /// Creates new inventory 
        /// </summary>
        public Inventory(Inventory inventory)
        {
            Clone(inventory);
        }

        private void Clone(Inventory inventory)
        {
            _width = inventory.Width;
            _height = inventory.Height;
            
            _slots = new InventorySlotData[inventory.Slots.Length];
            Array.Copy(inventory.Slots, _slots, inventory.Slots.Length);
        }

        private void InitializeInventorySlots(int width, int height)
        {
            _width = width;
            _height = height;
            int inventorySize = width * height;
            int tempWidth = 0;
            int tempHeight = 0;
            
            _slots = new InventorySlotData[inventorySize];

            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i].XPosition = tempWidth;
                _slots[i].YPosition = tempHeight;
                _slots[i].Item = null;
                
                tempWidth++;
                if (tempWidth >= width)
                {
                    tempWidth = 0;
                    tempHeight++;
                }
            }
        }

        /// <summary>
        /// Checks for adding an item on a specified position
        /// </summary>
        public bool CanAddItem(Item item, Vector2Int position)
        {
            if (item == null) 
                return false;
            
            if (item.Size.x <= 0 || item.Size.y <= 0) 
                throw new ArgumentException("Item size must be positive");
            
            if (Contains(item)) 
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

            SetItemAt(item, position);

            OnAdded?.Invoke(item, position);
            return true;
        }

        private void SetItemAt(Item item, Vector2Int position)
        {
            int startX = position.x;
            int startY = position.y;
            int endX = startX + item.Size.x;
            int endY = startY + item.Size.y;

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    int index = y * _width + x;
                    _slots[index].Item = item;
                }
            }
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
            
            if (item.Size.x <= 0 || item.Size.y <= 0) 
                throw new ArgumentException("Item size must be positive");
            
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
            
            if (item.Size.x <= 0 || item.Size.y <= 0) 
                throw new ArgumentException("Item size must be positive");
            
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

        /// <summary>
        /// Checks if the specified element exists
        /// </summary>
        public bool Contains(Item item)
        {
            if (item == null) 
                return false;
            
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Equals(item))
                    return true;
            }
            
            return false;
        }

        /// <summary>
        /// Checks if the specified position is occupied
        /// </summary>
        public bool IsOccupied(Vector2Int position)
        {
            return IsOccupied(position.x, position.y);
        }

        public bool IsOccupied(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return true;

            int index = y * _width + x;
            return _slots[index].Item != null;
        }

        /// <summary>
        /// Checks if the specified position is free
        /// </summary>
        public bool IsFree(Vector2Int position)
        {
            return IsFree(position.x, position.y);
        }

        public bool IsFree(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return false;

            int index = y * _width + x;
            return _slots[index].Item == null;
        }

        /// <summary>
        /// Removes specified item
        /// </summary>
        public bool RemoveItem(Item item)
        {
            if (item == null) return false;
            
            bool hasItem = false;
            Vector2Int position = Vector2Int.zero;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Equals(item))
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
                OnRemoved?.Invoke(item, position);
            
            return hasItem;
        }

        public bool RemoveItem(Item item, out Vector2Int position)
        {
            if (item == null)
            {
                position = default;
                return false;
            }

            bool hasItem = false;
            position = default;

            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Equals(item))
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

        public bool TryGetItem(Vector2Int position, out Item item)
        {
            return TryGetItem(position.x, position.y, out item);
        }

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
            
            var positions = new List<Vector2Int>();
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int index = y * _width + x;
                    
                    if (_slots[index].Item != null && _slots[index].Item.Equals(item))
                        positions.Add(new Vector2Int(x, y));
                }
            }

            if (positions.Count == 0) 
                throw new KeyNotFoundException("Item not found");

            return positions.ToArray();
        }

        public bool TryGetPositions(Item item, out Vector2Int[] positions)
        {
            if (item == null)
            {
                positions = null;
                return false;
            }

            var posList = new List<Vector2Int>();
            
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    int index = y * _width + x;
                    
                    if (_slots[index].Item != null && _slots[index].Item.Equals(item))
                        posList.Add(new Vector2Int(x, y));
                }
            }

            if (posList.Count == 0)
            {
                positions = null;
                return false;
            }

            positions = posList.ToArray();
            return true;
        }

        /// <summary>
        /// Clears all items 
        /// </summary>
        public void Clear()
        {
            if (Count == 0) 
                return;

            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Item = null;
            
            OnCleared?.Invoke();
        }

        /// <summary>
        /// Returns count of items with a specified name
        /// </summary>
        public int GetItemCount(string name)
        {
            var uniqueItems = new HashSet<Item>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Name == name)
                    uniqueItems.Add(_slots[i].Item);
            }
            return uniqueItems.Count;
        }

        public bool MoveItem(Item item, Vector2Int position)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            
            if (!TryGetPositions(item, out var oldPositions))
                return false;
            
            Vector2Int oldOrigin = oldPositions[0];
            
            var tempSlots = new List<int>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && _slots[i].Item.Equals(item))
                {
                    _slots[i].Item = null;
                    tempSlots.Add(i);
                }
            }

            if (CanAddItem(item, position))
            {
                SetItemAt(item, position);
                OnMoved?.Invoke(item, position);
                return true;
            }

            foreach (int index in tempSlots)
            {
                _slots[index].Item = item;
            }

            return false;
        }

        /// <summary>
        /// Rearranges an inventory space with max free slots 
        /// </summary>
        public void OptimizeSpace()
        {
            var items = new List<Item>();
            foreach (var item in this)
            {
                items.Add(item);
            }
            
            items.Sort((a, b) =>
            {
                int res = (b.Size.x * b.Size.y).CompareTo(a.Size.x * a.Size.y);
                if (res != 0) return res;
                return string.Compare(a.Name, b.Name, StringComparison.Ordinal);
            });

            Clear();

            foreach (var item in items)
                AddItem(item);
        }

        /// <summary>
        /// Iterates by all items 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<Item> GetEnumerator()
        {
            var uniqueItems = new HashSet<Item>();
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item != null && uniqueItems.Add(_slots[i].Item))
                    yield return _slots[i].Item;
            }
        }

        /// <summary>
        /// Copies items to a specified matrix
        /// </summary>
        public void CopyTo(Item[,] matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            
            int matrixWidth = matrix.GetLength(0);
            int matrixHeight = matrix.GetLength(1);
            
            int copyWidth = Math.Min(_width, matrixWidth);
            int copyHeight = Math.Min(_height, matrixHeight);

            for (int y = 0; y < copyHeight; y++)
            {
                for (int x = 0; x < copyWidth; x++)
                    matrix[x, y] = _slots[y * _width + x].Item;
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
                if (y < _height - 1) sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}