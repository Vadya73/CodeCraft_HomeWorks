using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
        public int Count => Width * Height;
        public InventorySlotData[] Slots => _slots;

        public Inventory(int width, int height)
        {
            InitializeInventorySlots(width, height);
        }

        public Inventory(int width, int height, params KeyValuePair<Item, Vector2Int>[] items)
        {
            InitializeInventorySlots(width, height);
            
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                AddItem(item.Key, item.Value);
            }
        }

        public Inventory(int width, int height, params Item[] items)
        {
            InitializeInventorySlots(width, height);
            
            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                AddItem(item);
            }
        }

        public Inventory(int width, int height, IEnumerable<KeyValuePair<Item, Vector2Int>> items)
        {
            InitializeInventorySlots(width, height);
            
            foreach (var item in items)
                AddItem(item.Key, item.Value);
        }

        public Inventory(int width, int height, IEnumerable<Item> items)
        {
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
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].XPosition != position.x || _slots[i].YPosition != position.y)
                    continue;

                if (_slots[i].Item == null)
                    return true;
            }

            return false;
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
            for (int i = 0; i < _slots.Length; i++)
            {
                
            }
            OnAdded?.Invoke(item, position);
            return true;
        }

        public bool AddItem(Item item, int startX, int startY)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks for adding an item on a free position
        /// </summary>
        public bool CanAddItem(Item item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds an item on a free position
        /// </summary>
        public bool AddItem(Item item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a free position for a specified item
        /// </summary>
        public bool FindFreePosition(Item item, out Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool FindFreePosition(Vector2Int size, out Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool FindFreePosition(int sizeX, int sizeY, out Vector2Int position)
        {
            throw new NotImplementedException();
        }

        private bool IsFreeSpace(int startX, int startY, int endX, int endY)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the specified element exists
        /// </summary>
        public bool Contains(Item item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the specified position is occupied
        /// </summary>
        public bool IsOccupied(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsOccupied(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if the specified position is free
        /// </summary>
        public bool IsFree(Vector2Int position)
        {
            throw new NotImplementedException();
        }

        public bool IsFree(int x, int y)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes specified item
        /// </summary>
        public bool RemoveItem(Item item)
        {
            bool hasItem = false; 
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item.Equals(item))
                {
                    hasItem = true;
                    OnRemoved?.Invoke(item, new Vector2Int(_slots[i].XPosition, _slots[i].YPosition));
                    _slots[i].Item = null;
                }
            }
            
            return hasItem;
        }

        public bool RemoveItem(Item item, out Vector2Int position)
        {
            bool hasItem = false;
            for (int i = 0; i < _slots.Length; i++)
            {
                if(_slots[i].Item.Equals(item))
                {
                    position = new Vector2Int(_slots[i].XPosition, _slots[i].YPosition);
                    OnRemoved?.Invoke(item, position);

                    for (int j = 0; j < _slots.Length; j++)
                    {
                        if(_slots[j].Item.Equals(item))
                            _slots[j].Item = null;
                    }

                    hasItem = true;
                    break;
                }
            }
            
            position = default;
            return hasItem;
        }

        /// <summary>
        /// Returns an item at specified position 
        /// </summary>
        public Item GetItem(Vector2Int position)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_slots[i].XPosition != position.x || _slots[i].YPosition != position.y)
                    continue;
                
                return _slots[i].Item;
            }
            return null;
        }

        public Item GetItem(int x, int y)
        {
            for (int i = 0; i < Count; i++)
            {
                if(_slots[i].XPosition != x || _slots[i].YPosition != y)
                    continue;
                
                return _slots[i].Item;
            }
            return null;
        }

        public bool TryGetItem(Vector2Int position, out Item item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_slots[i].XPosition != position.x || _slots[i].YPosition != position.y)
                {
                    continue;
                }
                item = _slots[i].Item;
                return true;
            }
            
            item = null;
            return false;
        }

        public bool TryGetItem(int x, int y, out Item item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (_slots[i].XPosition != x || _slots[i].YPosition != y)
                {
                    continue;
                }
                item = _slots[i].Item;
                return true;
            }
            
            item = null;
            return false;
        }

        /// <summary>
        /// Returns positions of a specified item 
        /// </summary>
        public Vector2Int[] GetPositions(Item item)
        {
            int itemSize = 0;
            for (int i = 0; i < Count; i++)
            {
                if (_slots[i].Item.Equals(item))
                    itemSize++;
            }
            
            Vector2Int[] positions = new Vector2Int[itemSize];
            int posSize = 0;
            for (int i = 0; i < Count; i++)
            {
                if (_slots[i].Item.Equals(item))
                {
                    positions[posSize] = new Vector2Int(_slots[i].XPosition, _slots[i].YPosition);
                    posSize++;
                }
            }
            return positions;
        }

        public bool TryGetPositions(Item item, out Vector2Int[] positions)
        {
            int size = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item.Equals(item))
                    size++;
            }

            if (size == 0)
            {
                positions = Array.Empty<Vector2Int>();
                return false;
            }

            positions = new Vector2Int[size];
                
            size = 0;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Item.Equals(item))
                    positions[size++] = new Vector2Int(_slots[i].XPosition, _slots[i].YPosition);
            }
                
            return true;
        }

        /// <summary>
        /// Clears all items 
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Item = null;
        }

        /// <summary>
        /// Returns count of items with a specified name
        /// </summary>
        public int GetItemCount(string name)
        {
            // int itemsCount = 0;
            // Item[] hasItems;
            //
            // for (int i = 0; i < _slots.Length; i++)
            // {
            //     if (_slots[i].Item != null && _slots[i].Item.Equals(hasItems)) ;
            // }
            return 1;
        }

        public bool MoveItem(Item item, Vector2Int position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Rearranges an inventory space with max free slots 
        /// </summary>
        public void OptimizeSpace()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Iterates by all items 
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Item> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Copies items to a specified matrix
        /// </summary>
        public void CopyTo(Item[,] matrix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns an inventory matrix in string format
        /// </summary>
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}