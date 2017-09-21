using System;
using System.Threading;
using System.Threading.Tasks;

namespace Logic
{
    internal sealed class TaskExecutor : IDisposable
    {
        private readonly PriorityQueue<ITask> _taskQueue;
        private readonly ManualResetEventSlim _newTaskEvent;

        public TaskExecutor(PriorityQueue<ITask> taskQueue)
        {
            _taskQueue = taskQueue ?? throw new ArgumentNullException(nameof(taskQueue));
            _newTaskEvent = new ManualResetEventSlim(false);
        }

        public async Task Run(CancellationToken token)
        {
            try
            {
                _taskQueue.ItemQueued += HandleItemEnqueued;

                await Task.Run(() =>
                {
                    while (true)
                    {
                        ExecuteAllTasks();

                        _newTaskEvent.Wait(token);
                        _newTaskEvent.Reset();
                    }
                }, token);
            }
            catch (OperationCanceledException)
            {
                ExecuteAllTasks();
            }
            finally
            {
                _taskQueue.ItemQueued -= HandleItemEnqueued;
            }
        }

        private void HandleItemEnqueued(object sender, EventArgs e)
        {
            _newTaskEvent.Set();
        }

        private void ExecuteAllTasks()
        {
            while (_taskQueue.TryDequeue(out var task))
            {
                task.Execute();
            }
        }

        public void Dispose()
        {
            _newTaskEvent.Dispose();
        }
    }
}
