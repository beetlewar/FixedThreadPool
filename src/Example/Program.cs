using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Logic;

namespace Example
{
    class Program
    {
        static void Main(string[] args)
        {
            var commandArgs = ParseCommandArgs(args);

            if (commandArgs == null)
            {
                PrintHelp();
                return;
            }

            Console.WriteLine($"Starting {commandArgs.NumTasks} tasks with {commandArgs.NumThreads} threads.");
            Console.WriteLine();

            try
            {
                var threadPool = new FixedThreadPool(commandArgs.NumThreads);

                var rnd = new Random(1);

                var stdWriters = Enumerable.Range(1, commandArgs.NumTasks)
                    .Select(i => new StdOutWriter(GeneratePriority(rnd), commandArgs.Delay));

                var sw = new Stopwatch();
                sw.Start();

                foreach (var stdWriter in stdWriters)
                {
                    threadPool.Execute(stdWriter, stdWriter.Priority);
                }

                threadPool.Stop();

                sw.Stop();

                Console.WriteLine();
                Console.WriteLine(sw.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine();

                PrintHelp();
            }
        }

        private static CommandArgs ParseCommandArgs(string[] args)
        {
            var commandArgs = new CommandArgs();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--threads":
                        i++;
                        ReadNumThreads(args, i, commandArgs);
                        break;
                    case "--tasks":
                        i++;
                        ReadNumTasks(args, i, commandArgs);
                        break;
                    case "--delay":
                        i++;
                        ReadDelay(args, i, commandArgs);
                        break;
                    default:
                        return null;
                }
            }

            return commandArgs;
        }

        private static void ReadNumThreads(string[] args, int i, CommandArgs target)
        {
            var numThreads = ReadInt(args, i);
            if (numThreads.HasValue)
            {
                target.NumThreads = numThreads.Value;
            }
        }

        private static int? ReadInt(string[] args, int index)
        {
            if (args.Length <= index)
            {
                return null;
            }

            if (int.TryParse(args[index], out var i))
            {
                return i;
            }

            return null;
        }

        private static void ReadNumTasks(string[] args, int i, CommandArgs target)
        {
            var numTasks = ReadInt(args, i);
            if (numTasks.HasValue)
            {
                target.NumTasks = numTasks.Value;
            }
        }

        private static void ReadDelay(string[] args, int i, CommandArgs target)
        {
            target.Delay = ReadInt(args, i);
        }

        private static void PrintHelp()
        {
            Console.WriteLine("--threads [num_threads]");
            Console.WriteLine("--tasks [num_tasks]");
            Console.WriteLine("--delay [msec]");
        }

        private static Priority GeneratePriority(Random random)
        {
            var d = random.NextDouble();

            if (d < 0.6)
            {
                return Priority.HIGH;
            }

            return d < 0.95 ? Priority.NORMAL : Priority.LOW;
        }

        private class CommandArgs
        {
            public int NumThreads { get; set; }

            public int NumTasks { get; set; }

            public int? Delay { get; set; }

            public CommandArgs()
            {
                NumThreads = 1;
                NumTasks = 1;
            }
        }

        private class StdOutWriter : ITask
        {
            private readonly int? _delay;

            public Priority Priority { get; }

            public StdOutWriter(Priority priority, int? delay)
            {
                _delay = delay;
                Priority = priority;
            }

            public void Execute()
            {
                Console.WriteLine($"{Priority}\t{Thread.CurrentThread.ManagedThreadId}");

                if (_delay.HasValue)
                {
                    Thread.Sleep(_delay.Value);
                }
            }
        }
    }
}
