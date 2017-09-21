using System;
using System.Collections.Generic;

namespace Logic
{
    internal sealed class PriorityQueue<T>
    {
        private const int NumOfHighItemsInARow = 3;

        private readonly object _sync = new object();
        private readonly Queue<T> _highQueue = new Queue<T>();
        private readonly Queue<T> _normalQueue = new Queue<T>();
        private readonly Queue<T> _lowQueue = new Queue<T>();

        private int _numOfHighSelections;

        public event EventHandler ItemQueued;

        public void Enqueue(T item, Priority priority)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            Queue<T> targetQueue;
            switch (priority)
            {
                case Priority.HIGH:
                    targetQueue = _highQueue;
                    break;
                case Priority.NORMAL:
                    targetQueue = _normalQueue;
                    break;
                case Priority.LOW:
                    targetQueue = _lowQueue;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            lock (_sync)
            {
                targetQueue.Enqueue(item);
            }

            ItemQueued?.Invoke(this, EventArgs.Empty);
        }

        public bool TryDequeue(out T item)
        {
            lock (_sync)
            {
                var queue = SelectQueue();

                if (queue == null)
                {
                    item = default(T);
                    return false;
                }

                item = queue.Dequeue();
                return true;
            }
        }

        private Queue<T> SelectQueue()
        {
            if (_highQueue.Count > 0 && (_numOfHighSelections < NumOfHighItemsInARow || _normalQueue.Count == 0))
            {
                _numOfHighSelections++;
                return _highQueue;
            }

            if (_normalQueue.Count > 0)
            {
                _numOfHighSelections = 0;
                return _normalQueue;
            }

            return _lowQueue.Count > 0 ? _lowQueue : null;
        }
    }
}
