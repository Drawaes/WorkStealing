using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleApp1
{
    public class Worker
    {
        private int _reductionPoint = 5000;
        private int _tasksDone = 0;
        private Random _random;
        private ConcurrentQueue<Action> _localQueue = new ConcurrentQueue<Action>();
        private ConcurrentQueue<Action>[] _globalQueues;
        private Worker[] _allWorkers;

        public Worker(int id, ConcurrentQueue<Action>[] globalQueues, Worker[] allWorkers)
        {
            _random = new Random(id);
            _allWorkers = allWorkers;
            _globalQueues = globalQueues;
            _localQueue.Enqueue(Sleep);

        }

        public int TasksDone => _tasksDone;

        public Thread Thread { get; set; }

        internal void Start()
        {
            var newThread = new Thread(RunThread);
            newThread.Start();
            Thread = newThread;
        }

        private void DecideWhatToDoNext()
        {
            _tasksDone++;
            var rnd = _random.Next(2);
            if (_tasksDone >= _reductionPoint)
            {
                rnd--;
            }
            else
            {
                rnd++;
            }
            if (rnd <= 0) return;

            var whichTask = _random.Next(3);
            for (var i = 0; i < rnd; i++)
            {
                switch (whichTask)
                {
                    case 0:
                        _localQueue.Enqueue(Spin);
                        break;
                    case 1:
                        _localQueue.Enqueue(Sleep);
                        break;
                    case 2:
                        _globalQueues[Program.CurrentExecutionId >> 2].Enqueue(Spin);
                        break;
                    case 3:
                        _globalQueues[Program.CurrentExecutionId >> 2].Enqueue(Sleep);
                        break;
                }
            }
        }

        public void RunThread(object state)
        {
            while (true)
            {
                var action = GetNextTask();
                if (action == null)
                {
                    Thread.SpinWait(500);
                    action = GetNextTask();
                    if (action == null) return;
                }
                action();
                DecideWhatToDoNext();
            }
        }

        public Action GetNextTask()
        {
            if (_localQueue.TryDequeue(out var action)) return action;

            //Steal from out "local" global
            var queueBucket = Program.CurrentExecutionId >> 2;
            if (_globalQueues[queueBucket].TryDequeue(out action)) return action;

            var rndStart = _random.Next(_allWorkers.Length - 1);
            var counter = _allWorkers.Length;
            while(counter > 0)
            {
                if (rndStart == _allWorkers.Length) rndStart = 0;
                if (_allWorkers[rndStart]._localQueue.TryDequeue(out action)) return action;
                rndStart++;
                counter--;
            }
            return null;
        }

        public void Sleep() => Thread.Sleep(10);
        public void Spin() => Thread.SpinWait(1000);
    }
}
