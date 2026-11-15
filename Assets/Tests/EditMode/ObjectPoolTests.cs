using NUnit.Framework;
using UnityEngine;
using DodgeGame.Core;

namespace DodgeGame.Tests.EditMode
{
    [TestFixture]
    public class ObjectPoolTests
    {
        private class TestComponent : MonoBehaviour { }

        private GameObject prefabGo;
        private TestComponent prefab;
        private GameObject poolRootGo;

        [SetUp]
        public void SetUp()
        {
            prefabGo = new GameObject("TestPrefab");
            prefab = prefabGo.AddComponent<TestComponent>();
            prefabGo.SetActive(false);

            poolRootGo = new GameObject("PoolRoot");
        }

        [TearDown]
        public void TearDown()
        {
            if (prefabGo != null) Object.DestroyImmediate(prefabGo);
            if (poolRootGo != null) Object.DestroyImmediate(poolRootGo);
        }

        [Test]
        public void InitialCapacity_PrewarmsInactiveInstances()
        {
            var pool = new ObjectPool<TestComponent>(prefab, initialCapacity: 5, maxSize: 10, parent: poolRootGo.transform);

            Assert.AreEqual(5, pool.CountInactive, "Pool should prewarm initialCapacity items.");
            Assert.AreEqual(5, pool.CountAll, "CountAll should equal initial prewarmed instances.");

            pool.Clear();
        }

        [Test]
        public void GetAndRelease_RecyclesInstanceProperly()
        {
            var pool = new ObjectPool<TestComponent>(prefab, initialCapacity: 3, maxSize: 5, parent: poolRootGo.transform);

            var item1 = pool.Get();
            Assert.IsTrue(item1.gameObject.activeSelf, "Retrieved item must be active.");
            Assert.AreEqual(2, pool.CountInactive, "Inactive count should decrease by 1.");

            pool.Release(item1);
            Assert.IsFalse(item1.gameObject.activeSelf, "Released item must be deactivated.");
            Assert.AreEqual(3, pool.CountInactive, "Released item must return to pool.");

            var item2 = pool.Get();
            Assert.AreSame(item1, item2, "Pooled item must be reused when Get() is called again.");

            pool.Release(item2);
            pool.Clear();
        }

        [Test]
        public void Pool_DoesNotGrowBeyondMaxSize()
        {
            int max = 4;
            var pool = new ObjectPool<TestComponent>(prefab, initialCapacity: 2, maxSize: max, parent: poolRootGo.transform);

            // Exhaust pool and get extra instances
            var i1 = pool.Get();
            var i2 = pool.Get();
            var i3 = pool.Get();
            var i4 = pool.Get();
            var i5 = pool.Get();
            var i6 = pool.Get();

            Assert.AreEqual(6, pool.CountAll, "CountAll should reflect 6 instantiated items.");

            // Release all 6 items back into the pool of maxSize 4
            pool.Release(i1);
            pool.Release(i2);
            pool.Release(i3);
            pool.Release(i4);
            pool.Release(i5);
            pool.Release(i6);

            Assert.AreEqual(max, pool.CountInactive, "Inactive pool count must not exceed maxSize.");
            Assert.LessOrEqual(pool.CountAll, max, "CountAll must not exceed maxSize after excess destruction.");

            pool.Clear();
        }
    }
}
