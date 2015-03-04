using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SPRITE.Utility
{
    namespace Concurrency
    {
        public class CustomizedTask
        {
            private String taskName;
            private Boolean isRepeated;
            private Boolean isStopped;
            public delegate void TaskInvoker();
            private TaskInvoker taskInvoker;

            public CustomizedTask(String taskName, TaskInvoker taskInvoker, Boolean isRepeated, Boolean isStopped)
            {
                this.taskName = taskName;
                this.taskInvoker = taskInvoker;
                this.isRepeated = isRepeated;
                this.isStopped = isStopped;
            }

            public void Start() { this.isStopped = false; }
            public void Stop() { this.isStopped = true; }

            public TaskInvoker GetTaskInvoker() { return this.taskInvoker; }
            public String GetTaskName() { return this.taskName; }
            public Boolean IsStopped() { return this.isStopped; }
            public Boolean IsRepeated() { return this.isRepeated; }
        }

        public class ConcurrentThreadPool
        {
            // containers
            private Queue<CustomizedTask> taskQueue = new Queue<CustomizedTask>();
            private List<Thread> availableThreads = new List<Thread>();
            private Dictionary<String, CustomizedTask> taskMap = new Dictionary<String, CustomizedTask>();

            // flags and locks
            private bool _stop = false;
            private bool _stop2 = false;
            private Object thisLock = new Object(); // mutex

            // counter
            private int count = 0;
            private int maxSize = 0;

            // singleton attibutes
            private static object singletonLock = new object();
            private static ConcurrentThreadPool instance = null;

            public bool IsActivated
            {
                get { return !this._stop2; }
            }

            public static ConcurrentThreadPool GetInstance(int count)
            {
                if (instance == null)
                {
                    lock (singletonLock)
                    {
                        instance = new ConcurrentThreadPool(count);
                    }
                }

                return instance;
            }

            private ConcurrentThreadPool(int count)
            {
                this.maxSize = count;
                this.count = count;

                // allocate 3 available threads in the thread pool
                for (int i = 0; i < this.maxSize; i++)
                {
                    availableThreads.Add(
                        new Thread(
                            delegate()
                            {
                                while (!_stop)
                                {
                                    int res = 0;
                                    CustomizedTask c = null;
                                    lock (thisLock)
                                    {
                                        if (taskQueue.Count == 0)
                                            res = 1;
                                        else
                                        {
                                            res = 2;
                                            c = taskQueue.Dequeue(); // dequeue task from task queue
                                            taskMap[c.GetTaskName()] = c; // log in the dictionary
                                        }
                                    }

                                    switch (res)
                                    { 
                                        case 1:
                                            Thread.Sleep(1000); // sleep
                                            break;
                                        case 2:
                                        {
                                            while (!_stop && !_stop2 && c.IsRepeated())
                                            {
                                                if (c.IsStopped())
                                                {
                                                    Thread.Sleep(1000); // sleep
                                                    continue;
                                                }

                                                c.GetTaskInvoker()(); // invoke function
                                            }
                                            
                                            taskMap.Remove(c.GetTaskName()); // remove from dictionary

                                            Interlocked.Increment(ref this.count);

                                            break;
                                        }
                                    }
                                }
                            }
                        )
                    );

                    availableThreads[i].Start();
                }
            }

            public bool HasNoTask
            {
                get { return this.count == this.maxSize; }
            }

            public int GetCount() { return this.count; }

            public void AddTask(CustomizedTask c)
            {
                lock (thisLock)
                {
                    taskQueue.Enqueue(c);
                }

                Interlocked.Decrement(ref this.count);
            }

            public void StartThread(String id)
            {
                lock (thisLock)
                {
                    taskMap[id].Start();
                }
            }

            public void StopThread(String id)
            {
                lock (thisLock)
                {
                    taskMap[id].Stop();
                }
            }

            public void KillAll()
            {
                lock (thisLock)
                {
                    _stop = true;
                }
            }

            public void ActivateAll()
            {
                lock (thisLock)
                {
                    _stop2 = false;
                }
            }

            public void DeactivateAll()
            {
                lock (thisLock)
                {
                    _stop2 = true;
                }
            }

            public void PauseAll()
            {
                lock (thisLock)
                {
                    for (int index = 0; index < taskMap.Count; index++)
                    {
                        taskMap.ElementAt(index).Value.Stop();
                    }
                }
            }

            public void StartAll()
            {
                lock (thisLock)
                {
                    for (int index = 0; index < taskMap.Count; index++)
                    {
                        taskMap.ElementAt(index).Value.Start();
                    }
                }
            }
        }
    }
}
