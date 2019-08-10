using System;
using System.Collections.Generic;
using System.Threading;

namespace HandBrakeEncoder
{
    public class FileMover : IDisposable
    {
        private static int MAX_SLEEP_INTERVALS = 200;
        private volatile Queue<FileMoverWorkItem> workItems = new Queue<FileMoverWorkItem>();
        private Thread workerThread = null;
        private volatile object workerThreadLock = new object();
        private static readonly FileLogger logger = FileLogger.GetInstance();
        private static bool IsThreadRunning = false;

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
        public void AddWorkItem(FileMoverWorkItem workItem)
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
                if (!IsThreadRunning)
                {
                    // Only start worker thread if no worker thread currently exists
                    workerThread = new Thread(new ThreadStart(Move));
                    workerThread.Start();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Move()
        {
            int counter = 0;
            while (true)
            {
                FileMoverWorkItem workItem = null;
                lock (workItems)
                {
                    // Get the item
                    if (workItems.Count > 0 && workItems.Peek() != null) 
                    { 
                        workItem = workItems.Dequeue();
                    }

                    if (workItem == null)
                    {
                        if (counter == MAX_SLEEP_INTERVALS)
                        {
                            // Max running time so notify that we are not using the thread anymore
                            IsThreadRunning = false;
                            return;
                        }
                        
                        // Keep looking
                        counter++;
                        continue;
                    }
                }

                // We have an item so continue working
                logger.Log("Found item to move.");

                // Do stuff with the file
                CopyFile(workItem);

                logger.Log("Finished moving item");
            }   
        }

        private void CopyFile(FileMoverWorkItem workItem)
        {
            System.IO.File.Copy(workItem.EncodedFilePath, workItem.DestinationFilePath, true);
        }

        public void Dispose()
        {
            StopWorkerThread();

            // Should go through and log all the files that haven't been encoded yet. Maybe a database?
        }

    }
}
    