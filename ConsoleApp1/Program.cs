using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ConsoleApp1
{
    class Program
    {
        static ConcurrentQueue<Action>[] _globalQueues = new ConcurrentQueue<Action>[8 >> 2];
        static Worker[] _threads = new Worker[16];
        delegate int GetProcDelegate();
        static GetProcDelegate GetProc;

        static void Main(string[] args)
        {
            var environment = Type.GetType("System.Environment");
            var props = environment.GetMethod("get_CurrentProcessorNumber", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            GetProc = (GetProcDelegate) props.CreateDelegate(typeof(GetProcDelegate));


            for (var i = 0; i < _globalQueues.Length;i++)
            {
                _globalQueues[i] = new ConcurrentQueue<Action>();
            }

            for(var i = 0; i < _threads.Length;i++)
            {
                _threads[i] = new Worker(i, _globalQueues, _threads);
            }
            for(var i = 0; i<_threads.Length;i++)
            {
                _threads[i].Start();
            }
            
            for(var i = 0; i < _threads.Length;i++)
            {
                _threads[i].Thread.Join();
                Console.WriteLine($"{i + 1} thread finished tasks done {_threads[i].TasksDone}");
            }
            Console.WriteLine("Hello World!");
        }

        // The upper bits of t_executionIdCache are the executionId. The lower bits of
        // the t_executionIdCache are counting down to get it periodically refreshed.
        // TODO: Consider flushing the executionIdCache on Wait operations or similar 
        // actions that are likely to result in changing the executing core
        [ThreadStatic]
        static int t_executionIdCache;

        const int ExecutionIdCacheShift = 16;
        const int ExecutionIdCacheCountDownMask = (1 << ExecutionIdCacheShift) - 1;
        const int ExecutionIdRefreshRate = 5000;

        private static int RefreshExecutionId()
        {
            int executionId = GetProc();

            // On Unix, CurrentProcessorNumber is implemented in terms of sched_getcpu, which
            // doesn't exist on all platforms.  On those it doesn't exist on, GetCurrentProcessorNumber
            // returns -1.  As a fallback in that case and to spread the threads across the buckets
            // by default, we use the current managed thread ID as a proxy.
            if (executionId < 0) executionId = Environment.CurrentManagedThreadId;

            Debug.Assert(ExecutionIdRefreshRate <= ExecutionIdCacheCountDownMask);

            // Mask with Int32.MaxValue to ensure the execution Id is not negative
            t_executionIdCache = ((executionId << ExecutionIdCacheShift) & Int32.MaxValue) | ExecutionIdRefreshRate;

            return executionId;
        }

        // Cached processor number used as a hint for which per-core stack to access. It is periodically
        // refreshed to trail the actual thread core affinity.
        public static int CurrentExecutionId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                int executionIdCache = t_executionIdCache--;
                if ((executionIdCache & ExecutionIdCacheCountDownMask) == 0)
                    return RefreshExecutionId();
                return (executionIdCache >> ExecutionIdCacheShift);
            }
        }
    }
}
