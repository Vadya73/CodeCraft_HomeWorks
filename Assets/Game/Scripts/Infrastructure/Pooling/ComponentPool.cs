using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game
{
    public sealed class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _container;
        private readonly Queue<T> _items = new();

        public ComponentPool(T prefab, Transform container, int initialSize)
        {
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _container = container;

            for (int i = 0; i < initialSize; i++)
                _items.Enqueue(CreateItem());
        }

        public T Get()
        {
            T item = _items.Count > 0 ? _items.Dequeue() : CreateItem();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (!item)
                throw new ArgumentNullException(nameof(item));

            item.gameObject.SetActive(false);
            _items.Enqueue(item);
        }

        private T CreateItem()
        {
            T item = Object.Instantiate(_prefab, _container);
            item.gameObject.SetActive(false);
            return item;
        }
    }
}
