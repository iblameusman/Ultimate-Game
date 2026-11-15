using System;
using System.Collections.Generic;
using UnityEngine;

namespace DodgeGame.Core
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> pool;
        private readonly Action<T> onGet;
        private readonly Action<T> onRelease;
        private readonly int maxSize;

        public int CountInactive => pool.Count;
        public int MaxSize => maxSize;
        public int CountAll { get; private set; }

        public ObjectPool(T prefab, int initialCapacity = 10, int maxSize = 50, Transform parent = null, Action<T> onGet = null, Action<T> onRelease = null)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.onGet = onGet;
            this.onRelease = onRelease;
            this.maxSize = Mathf.Max(initialCapacity, maxSize);
            this.pool = new Stack<T>(initialCapacity);

            for (int i = 0; i < initialCapacity; i++)
            {
                T instance = CreateNewInstance();
                instance.gameObject.SetActive(false);
                pool.Push(instance);
            }
        }

        private T CreateNewInstance()
        {
            CountAll++;
            T instance = UnityEngine.Object.Instantiate(prefab, parent);
            return instance;
        }

        public T Get()
        {
            T instance = pool.Count > 0 ? pool.Pop() : CreateNewInstance();
            instance.gameObject.SetActive(true);
            onGet?.Invoke(instance);
            return instance;
        }

        public void Release(T instance)
        {
            if (instance == null) return;

            onRelease?.Invoke(instance);
            instance.gameObject.SetActive(false);
            if (parent != null)
            {
                instance.transform.SetParent(parent);
            }

            if (pool.Count < maxSize)
            {
                pool.Push(instance);
            }
            else
            {
                CountAll--;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(instance.gameObject);
                else
                    UnityEngine.Object.DestroyImmediate(instance.gameObject);
            }
        }

        public void Clear()
        {
            while (pool.Count > 0)
            {
                T item = pool.Pop();
                if (item != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(item.gameObject);
                    else
                        UnityEngine.Object.DestroyImmediate(item.gameObject);
                }
            }
        }
    }
}
