﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace HandBrakeEncoder
{
    public class FileMover : IDisposable
    {
        private static int MOVER_SLEEP_TIMER_MS = 5000; // 5 seconds

        private volatile Queue<HandBrakeWorkItem> workItems = new Queue<HandBrakeWorkItem>();
        private Thread workerThread = null;
        private volatile object workerThreadLock = new object();
        private HandBrakeEventLogger eventLog = HandBrakeEventLogger.GetInstance();

        /// <summary>
        /// Stops the worker thread
        /// </summary>
        private void StopWorkerThread()
        {
            lock (workerThreadLock)
            {
                if (workerThread != null && workerThread.IsAlive)
                {
                    workerThread.Abort();
                }
            }
        }

        /// <summary>
        /// Adds a work item to be processed
        /// </summary>
        /// <param name="workItem"></param>
        public void AddWorkItem(HandBrakeWorkItem workItem)
        {
            lock (workItems)
            {
                workItems.Enqueue(workItem);
            }
            StartWorkerThread();
        }

        /// <summary>
        /// Starts a worker thread, which will begin looking
        /// </summary>
        private void StartWorkerThread()
        {
            lock (workerThreadLock)
            {
                if (workerThread == null || !workerThread.IsAlive)
                {
                    // Only start worker thread if no worker thread currently exists
                    workerThread = new Thread(new ThreadStart(Move));
                    workerThread.Start();
                }
            }
        }

        public void Dispose()
        {
            StopWorkerThread();
            
            // Should go through and log all the files that haven't been encoded yet. Maybe a database?
        }

        /// <summary>
        /// 
        /// </summary>
        private void Move()
        {
            while (true)
            {
                HandBrakeWorkItem workItem = null;
                lock (workItems)
                {
                    if (workItems.Peek() != null) 
                    { 
                        workItem = workItems.Dequeue();
                    }
                }

                if (workItem == null)
                {
                    eventLog.WriteEntry("No item in mover thread. Sleeping");
                    Thread.Sleep(MOVER_SLEEP_TIMER_MS);
                    continue;
                }

                eventLog.WriteEntry("Found item to move.");

                // Do stuff with the file

                eventLog.WriteEntry("Finished moving item");
            }
        }
    }
}
