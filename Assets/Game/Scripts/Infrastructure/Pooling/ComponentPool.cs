using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Scripts.Infrastructure.Pooling
{
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _container;
        private readonly Transform _inactiveContainer;
        private readonly Queue<T> _items = new();
        private readonly HashSet<T> _rentedItems = new();

        public ComponentPool(T prefab, Transform container, int initialSize)
        {
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _container = container;

            if (initialSize < 0)
                throw new ArgumentOutOfRangeException(nameof(initialSize), initialSize, "Initial pool size cannot be negative");

            GameObject inactiveContainer = new GameObject($"<{typeof(T).Name}PoolInactive>");
            inactiveContainer.SetActive(false);
            _inactiveContainer = inactiveContainer.transform;
            _inactiveContainer.SetParent(_container, false);

            for (int i = 0; i < initialSize; i++)
                _items.Enqueue(CreateItem());
        }

        public T Rent(Action<T> initialize)
        {
            if (initialize == null)
                throw new ArgumentNullException(nameof(initialize));

            T item = _items.Count > 0 ? _items.Dequeue() : CreateItem();

            if (!_rentedItems.Add(item))
                throw new InvalidOperationException("The pool attempted to rent an item that is already rented");

            try
            {
                initialize(item);
                item.gameObject.SetActive(true);
            }
            catch
            {
                item.gameObject.SetActive(false);
                _rentedItems.Remove(item);
                _items.Enqueue(item);
                throw;
            }

            return item;
        }

        public void Return(T item)
        {
            if (!item)
                throw new ArgumentNullException(nameof(item));

            if (!_rentedItems.Remove(item))
                throw new InvalidOperationException("The item cannot be returned because it is not rented from this pool or was already returned");

            item.gameObject.SetActive(false);
            _items.Enqueue(item);
        }

        private T CreateItem()
        {
            T item = Object.Instantiate(_prefab, _inactiveContainer);
            item.gameObject.SetActive(false);
            item.transform.SetParent(_container, false);
            return item;
        }
    }
}
