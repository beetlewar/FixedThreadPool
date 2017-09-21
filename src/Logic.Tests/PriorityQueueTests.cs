using System.Collections.Generic;
using NUnit.Framework;

namespace Logic.Tests
{
    [TestFixture]
    public class PriorityQueueTests
    {
        [Test]
        public void Should_Dequeue_Items_In_Expected_Order()
        {
            var queue = new PriorityQueue<int>();

            queue.Enqueue(1, Priority.HIGH);
            queue.Enqueue(2, Priority.NORMAL);
            queue.Enqueue(3, Priority.NORMAL);
            queue.Enqueue(4, Priority.HIGH);
            queue.Enqueue(5, Priority.HIGH);
            queue.Enqueue(6, Priority.HIGH);
            queue.Enqueue(7, Priority.LOW);
            queue.Enqueue(8, Priority.NORMAL);
            queue.Enqueue(9, Priority.HIGH);
            queue.Enqueue(10, Priority.HIGH);
            queue.Enqueue(11, Priority.NORMAL);

            var expectedItems = new[]
            {
                1, 4, 5, 2, 6, 9, 10, 3, 8, 11, 7
            };

            AssertItems(queue, expectedItems);
        }

        private static void AssertItems(PriorityQueue<int> queue, int[] expectedItems)
        {
            var actualItems = new List<int>();

            while (queue.TryDequeue(out var nextItem))
            {
                actualItems.Add(nextItem);
            }

            Assert.AreEqual(expectedItems, actualItems);
        }

        [Test]
        public void Should_Dequeue_High_Priority_Items_If_No_Other()
        {
            var queue = new PriorityQueue<int>();

            queue.Enqueue(1, Priority.HIGH);
            queue.Enqueue(2, Priority.HIGH);
            queue.Enqueue(3, Priority.HIGH);
            queue.Enqueue(4, Priority.HIGH);

            var expectedItems = new[] { 1, 2, 3, 4 };

            AssertItems(queue, expectedItems);
        }

        [Test]
        public void Should_Raise_ItemQueues_When_Items_Added()
        {
            var queue = new PriorityQueue<int>();

            var eventRaised = false;

            queue.ItemQueued += delegate
            {
                eventRaised = true;
            };

            queue.Enqueue(1, Priority.HIGH);

            Assert.True(eventRaised);
        }
    }
}
