using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Logic
{
    public sealed class FixedThreadPool
    {
        private readonly CancellationTokenSource _tokenSource;
        private readonly TaskExecutor[] _executors;
        private readonly Task[] _executorTasks;
        private readonly PriorityQueue<ITask> _queue;
        private bool _stopped;

        public FixedThreadPool(int workCount)
        {
            _tokenSource = new CancellationTokenSource();

            _queue = new PriorityQueue<ITask>();

            _executors = Enumerable.Range(1, workCount)
                .Select(i => new TaskExecutor(_queue))
                .ToArray();

            _executorTasks = _executors.Select(e => e.Run(_tokenSource.Token))
                .ToArray();
        }

        public bool Execute(ITask task, Priority priority)
        {
            if (_stopped)
            {
                return false;
            }

            _queue.Enqueue(task, priority);

            return true;
        }

        public void Stop()
        {
            _tokenSource.Cancel();

            Task.WaitAll(_executorTasks);

            _stopped = true;

            foreach (var executor in _executors)
            {
                executor.Dispose();
            }
        }
    }
}
