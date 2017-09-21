using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;

namespace Logic.Tests
{
    [TestFixture]
    public class FixedThreadPoolTests
    {
        [Test]
        public void Should_Execute_All_Tasks_On_Stop()
        {
            var tasks = Enumerable.Range(1, 1000).Select(i => MockRepository.GenerateMock<ITask>()).ToList();

            tasks.ForEach(task => task.Expect(t => t.Execute()).WhenCalled(mi => Thread.Sleep(1))); // искусственная задержка

            var threadPool = new FixedThreadPool(100);

            tasks.ForEach(task => threadPool.Execute(task, Priority.HIGH));

            threadPool.Stop();

            tasks.ForEach(task => task.VerifyAllExpectations());
        }

        [Test]
        public void Should_Not_Start_Task_When_Stopped()
        {
            var threadPool = new FixedThreadPool(1);

            threadPool.Stop();

            var enqueued = threadPool.Execute(MockRepository.GenerateStub<ITask>(), Priority.HIGH);

            Assert.False(enqueued);
        }

        [Test]
        public void Should_Start_While_Not_Stopped()
        {
            var threadPool = new FixedThreadPool(1);

            var enqueued = threadPool.Execute(MockRepository.GenerateStub<ITask>(), Priority.NORMAL);

            Assert.True(enqueued);
        }
    }
}
