using System;
using System.Threading;
using System.Threading.Tasks;

namespace SemaphoreSlimTaskQueue
{
    internal class DataTransferModel
    {
        public DateTime Data;
    }

    internal class DataManager
    {
        public static DataTransferModel DataManagerMethod()
        {
            return new DataTransferModel
            {
                Data = DateTime.Now
            };
        }
    }

    class Program
    {
        public static async void OnDataRecived(DataTransferModel dataTransferModel)
        {
            await _taskQueue.Enqueue(() => Task.Run(async () =>
            {
                await ProcessData(dataTransferModel);
            }));
        }

        private static async Task ProcessData(DataTransferModel dataTransferModel)
        {
            Console.WriteLine(dataTransferModel.Data.Ticks);
            await Task.Delay(5);
            await Task.Delay(5);
            await Task.Delay(5);
            await Task.Delay(5);
        }

        private static TaskQueue _taskQueue;

        static void Main(string[] args)
        {
            _taskQueue = new TaskQueue(4);

            int i = 0;
            while (i++ < 300)
            {
                OnDataRecived(DataManager.DataManagerMethod());
                Thread.Sleep(1);
            }


            Console.ReadKey();
            _taskQueue.Dispose();
            Console.WriteLine("Finish");
        }

        private static void Trace()
        {
            ThreadPool.GetAvailableThreads(out var workerThreads, out var completionPortThreads);
            var str = string.Format("Available thread count: worker = {0}, completion port = {1}", workerThreads, completionPortThreads);
            Console.WriteLine("[{0}] {1}", Thread.CurrentThread.ManagedThreadId, str);
        }
    }

    public class TaskQueue : IDisposable
    {
        private SemaphoreSlim semaphore;

        public TaskQueue() : this(1) { }

        public TaskQueue(int initialCount)
        {
            semaphore = new SemaphoreSlim(initialCount);
        }

        public async Task Enqueue(Func<Task> taskRuner)
        {
            await semaphore.WaitAsync();

            try
            {
                await taskRuner();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public void Dispose()
        {
            semaphore.Dispose();
        }
    }
}
