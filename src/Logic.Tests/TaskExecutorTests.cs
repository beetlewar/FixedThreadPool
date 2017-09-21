using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;

namespace Logic.Tests
{
    [TestFixture]
    public class TaskExecutorTests
    {
        [Test]
        public void Should_Execute_Tasks()
        {
            var task1 = MockRepository.GenerateMock<ITask>();
            var task2 = MockRepository.GenerateMock<ITask>();

            var queue = new PriorityQueue<ITask>();

            var executor = new TaskExecutor(queue);

            var tokenSource = new CancellationTokenSource();

            var executionTask = executor.Run(tokenSource.Token);

            Thread.Sleep(100);
            queue.Enqueue(task1, Priority.HIGH);

            Thread.Sleep(100);
            queue.Enqueue(task2, Priority.HIGH);

            tokenSource.Cancel();

            executionTask.GetAwaiter().GetResult();

            task1.VerifyAllExpectations();
            task2.VerifyAllExpectations();
        }

        [Test]
        public async Task Should_Execute_All_Tasks_Event_If_Token_Is_Cancelled()
        {
            var expectedTasks = Enumerable.Range(0, 1000).Select(i => MockRepository.GenerateMock<ITask>()).ToList();

            expectedTasks.ForEach(task => task.Expect(t => t.Execute()));

            var queue = new PriorityQueue<ITask>();

            expectedTasks.ForEach(task => queue.Enqueue(task, Priority.HIGH));

            var executor = new TaskExecutor(queue);

            await executor.Run(new CancellationToken(true));

            expectedTasks.ForEach(task => task.VerifyAllExpectations());
        }

        [Test]
        public void Should_Execute_New_Tasks()
        {
            var task = MockRepository.GenerateMock<ITask>();

            task.Expect(t => t.Execute());

            var queue = new PriorityQueue<ITask>();

            var executor = new TaskExecutor(queue);

            var tokenSource = new CancellationTokenSource();

            var executionTask = executor.Run(tokenSource.Token);

            Thread.Sleep(100);

            queue.Enqueue(task, Priority.HIGH);

            task.VerifyAllExpectations();

            tokenSource.Cancel();

            executionTask.GetAwaiter().GetResult();
        }
    }
}
