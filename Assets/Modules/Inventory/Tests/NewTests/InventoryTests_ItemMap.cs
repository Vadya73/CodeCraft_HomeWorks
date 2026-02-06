using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Modules.Inventories.Tests
{
    public sealed partial class InventoryTests
    {
        [Test]
        public void CopyConstructor_CloneIsIndependentFromSource()
        {
            var itemA = new Item("A", 2, 2);
            var itemB = new Item("B", 1, 1);

            var original = new Inventory(4, 4,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(3, 3))
            );

            var clone = new Inventory(original);

            Assert.AreEqual(original.Width, clone.Width);
            Assert.AreEqual(original.Height, clone.Height);
            Assert.AreEqual(original.Count, clone.Count);
            Assert.AreEqual(itemA, clone.GetItem(0, 0));
            Assert.AreEqual(itemB, clone.GetItem(3, 3));

            clone.RemoveItem(itemA, out _);

            Assert.IsFalse(clone.Contains(itemA));
            Assert.IsTrue(original.Contains(itemA));
            Assert.AreEqual(itemA, original.GetItem(0, 0));
            Assert.IsTrue(original.IsOccupied(1, 1));
        }

        [Test]
        public void RemoveItem_WhenNotLast_RemainingItemsStayAccessible()
        {
            var itemA = new Item("A", 1, 1);
            var itemB = new Item("B", 1, 1);
            var itemC = new Item("C", 1, 1);

            var inventory = new Inventory(4, 4,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(1, 0)),
                new KeyValuePair<Item, Vector2Int>(itemC, new Vector2Int(2, 0))
            );

            Assert.IsTrue(inventory.RemoveItem(itemA, out _));
            Assert.IsTrue(inventory.Contains(itemB));
            Assert.IsTrue(inventory.Contains(itemC));
            CollectionAssert.AreEqual(new[] { new Vector2Int(2, 0) }, inventory.GetPositions(itemC));

            Assert.IsTrue(inventory.RemoveItem(itemC, out _));
            Assert.AreEqual(1, inventory.Count);
            Assert.IsTrue(inventory.Contains(itemB));
        }

        [Test]
        public void MoveItem_Failed_RevertsSlotsAndPositions()
        {
            var itemA = new Item("A", 2, 2);
            var itemB = new Item("B", 2, 2);

            var inventory = new Inventory(4, 4,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(2, 0))
            );

            bool success = inventory.MoveItem(itemA, new Vector2Int(1, 0));

            Assert.IsFalse(success);
            Assert.AreEqual(itemA, inventory.GetItem(0, 0));
            Assert.AreEqual(itemA, inventory.GetItem(1, 1));
            Assert.AreEqual(itemB, inventory.GetItem(2, 0));
            CollectionAssert.AreEqual(new[]
            {
                new Vector2Int(0, 0),
                new Vector2Int(0, 1),
                new Vector2Int(1, 0),
                new Vector2Int(1, 1)
            }, inventory.GetPositions(itemA));
        }

        [Test]
        public void AddItem_AfterRemove_ItemIsTrackable()
        {
            var itemA = new Item("A", 1, 1);
            var itemB = new Item("B", 1, 1);
            var itemC = new Item("C", 1, 1);

            var inventory = new Inventory(3, 3,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(1, 0))
            );

            Assert.IsTrue(inventory.RemoveItem(itemA, out _));
            Assert.IsTrue(inventory.AddItem(itemC, new Vector2Int(0, 0)));
            Assert.IsTrue(inventory.Contains(itemB));
            Assert.IsTrue(inventory.Contains(itemC));
        }

        [Test]
        public void ItemMap_GrowsAndReusesRemovedSlots()
        {
            var inventory = new Inventory(10, 10);
            var items = new List<Item>();
            var positions = new List<Vector2Int>();

            for (int i = 0; i < 20; i++)
            {
                var item = new Item($"I{i}", 1, 1);
                var position = new Vector2Int(i % 10, i / 10);
                items.Add(item);
                positions.Add(position);
                Assert.IsTrue(inventory.AddItem(item, position));
            }

            int[] removeIndices = { 1, 3, 5, 7, 9 };
            foreach (int index in removeIndices)
            {
                Assert.IsTrue(inventory.RemoveItem(items[index], out var removedPosition));
                Assert.AreEqual(positions[index], removedPosition);
            }

            var newItems = new List<Item>();
            var newPositions = new List<Vector2Int>();

            for (int i = 0; i < removeIndices.Length; i++)
            {
                int index = removeIndices[i];
                var item = new Item($"N{i}", 1, 1);
                var position = positions[index];
                newItems.Add(item);
                newPositions.Add(position);
                Assert.IsTrue(inventory.AddItem(item, position));
            }

            foreach (int index in removeIndices)
                Assert.IsFalse(inventory.Contains(items[index]));

            for (int i = 0; i < newItems.Count; i++)
            {
                Assert.IsTrue(inventory.Contains(newItems[i]));
                Assert.IsTrue(inventory.TryGetPositions(newItems[i], out var actualPositions));
                CollectionAssert.AreEqual(new[] { newPositions[i] }, actualPositions);
            }

            Assert.IsTrue(inventory.Contains(items[0]));
            CollectionAssert.AreEqual(new[] { positions[0] }, inventory.GetPositions(items[0]));
            Assert.AreEqual(20, inventory.Count);
        }

        [Test]
        public void Clear_RemovesMapEntriesAndAllowsReAdd()
        {
            var itemA = new Item("A", 1, 1);
            var itemB = new Item("B", 2, 1);

            var inventory = new Inventory(3, 3,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(1, 2))
            );

            inventory.Clear();

            Assert.IsFalse(inventory.TryGetPositions(itemA, out var positions));
            Assert.IsNull(positions);
            Assert.IsTrue(inventory.AddItem(itemA, new Vector2Int(2, 0)));
            CollectionAssert.AreEqual(new[] { new Vector2Int(2, 0) }, inventory.GetPositions(itemA));
        }

        [Test]
        public void OptimizeSpace_UpdatesItemPositionsMap()
        {
            var itemA = new Item("A", 2, 2);
            var itemB = new Item("B", 1, 3);
            var itemC = new Item("C", 1, 1);

            var inventory = new Inventory(4, 4,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(1, 1)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemC, new Vector2Int(3, 3))
            );

            inventory.OptimizeSpace();

            var items = new[] { itemA, itemB, itemC };
            foreach (var item in items)
            {
                Assert.IsTrue(inventory.TryGetPositions(item, out var positions));
                Assert.AreEqual(item.Size.x * item.Size.y, positions.Length);

                var unique = new HashSet<Vector2Int>(positions);
                Assert.AreEqual(positions.Length, unique.Count);

                foreach (var position in positions)
                    Assert.AreEqual(item, inventory.GetItem(position));
            }
        }

        [Test]
        public void MoveItem_Successful_OtherItemsRemainInPlace()
        {
            var mover = new Item("Mover", 2, 2);
            var other = new Item("Other", 1, 2);

            var inventory = new Inventory(5, 5,
                new KeyValuePair<Item, Vector2Int>(mover, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(other, new Vector2Int(4, 0))
            );

            var otherPositionsBefore = inventory.GetPositions(other);

            Assert.IsTrue(inventory.MoveItem(mover, new Vector2Int(1, 2)));

            CollectionAssert.AreEqual(otherPositionsBefore, inventory.GetPositions(other));
            foreach (var position in otherPositionsBefore)
                Assert.AreEqual(other, inventory.GetItem(position));

            CollectionAssert.AreEqual(new[]
            {
                new Vector2Int(1, 2),
                new Vector2Int(1, 3),
                new Vector2Int(2, 2),
                new Vector2Int(2, 3)
            }, inventory.GetPositions(mover));

            Assert.IsTrue(inventory.IsFree(0, 0));
            Assert.IsTrue(inventory.IsFree(1, 1));
        }

        [Test]
        public void CopyConstructor_MoveInCloneDoesNotAffectOriginal()
        {
            var itemA = new Item("A", 2, 2);
            var itemB = new Item("B", 1, 1);

            var original = new Inventory(4, 4,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(3, 3))
            );

            var clone = new Inventory(original);

            Assert.IsTrue(clone.MoveItem(itemA, new Vector2Int(2, 0)));

            Assert.AreEqual(itemA, original.GetItem(0, 0));
            Assert.IsTrue(original.IsOccupied(1, 1));
            Assert.IsNull(original.GetItem(2, 0));

            CollectionAssert.AreEqual(new[]
            {
                new Vector2Int(2, 0),
                new Vector2Int(2, 1),
                new Vector2Int(3, 0),
                new Vector2Int(3, 1)
            }, clone.GetPositions(itemA));
        }

        [Test]
        public void CopyConstructor_ClearInCloneDoesNotAffectOriginal()
        {
            var itemA = new Item("A", 1, 1);
            var itemB = new Item("B", 2, 1);

            var original = new Inventory(3, 3,
                new KeyValuePair<Item, Vector2Int>(itemA, new Vector2Int(0, 0)),
                new KeyValuePair<Item, Vector2Int>(itemB, new Vector2Int(1, 2))
            );

            var clone = new Inventory(original);
            clone.Clear();

            Assert.AreEqual(0, clone.Count);
            Assert.IsFalse(clone.Contains(itemA));
            Assert.IsFalse(clone.Contains(itemB));

            Assert.AreEqual(2, original.Count);
            Assert.IsTrue(original.Contains(itemA));
            Assert.IsTrue(original.Contains(itemB));
            Assert.AreEqual(itemA, original.GetItem(0, 0));
        }
    }
}
