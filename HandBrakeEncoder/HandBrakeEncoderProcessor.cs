using System;
using System.Collections.Generic;
using System.Threading;

namespace HandBrakeEncoder
{
    /// <summary>
    /// Takes items from a work queue and begins encoding them with Hand Brake.
    /// </summary>
    public class HandBrakeEncoderProcessor : IDisposable
    {
        private const int WORKER_THREAD_SLEEP_MS = 5000; // 5 seconds

        private static readonly HandBrakeEventLogger logger = HandBrakeEventLogger.GetInstance();
        private volatile Queue<HandBrakeWorkItem> workItems = new Queue<HandBrakeWorkItem>();
        private volatile object threadLock = new object();

        private Thread thread = null;

        private volatile FileMover fileMover = new FileMover();
        public FileMover FileMover => fileMover;

        public void AddWorkItem(HandBrakeWorkItem workItem)
        {
            lock (workItems)
            {
                workItems.Enqueue(workItem);
            }
        }

        /// <summary>
        /// Starts a working thread to begin looking for items to encode and start encoding them.
        /// It also kicks off another thread for the file move to being looking for files to move.
        /// </summary>
        public void StartWorkerThread()
        {
            lock (threadLock)
            {
                if (thread == null || !thread.IsAlive)
                {
                    thread = new Thread(new ThreadStart(ProcessWorkQueueAndEncode));
                }

                thread.Start();
                FileMover.StartWorkerThread();
            }
        }

        /// <summary>
        /// Goes through the work items and begins encoding.
        /// </summary>
        private void ProcessWorkQueueAndEncode()
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
                    logger.WriteEntry("Didn't find work item. Sleeping");
                    // Nothing to do. Take a break
                    Thread.Sleep(WORKER_THREAD_SLEEP_MS);
                    continue;
                }

                // Found a work item. Beginning Encoding
                logger.WriteEntry("Found Work Item. Beginning encoding");
                EncodeAndSendToFileMover(workItem);
                logger.WriteEntry("Finished Encodeing");
            }
        }

        /// <summary>
        /// Does the actual encoding work. 
        /// </summary>
        /// <param name="workItem">The workItem containing the necessary information to begin encoding</param>
        private void EncodeAndSendToFileMover(HandBrakeWorkItem workItem)
        {
            // Do the encoding

            // Send the workitem to the filemover to actually move the file
            SendWorkItemToFileMover(workItem);
        }

        /// <summary>
        /// Sends the work item to the file mover
        /// </summary>
        /// <param name="workItem">The workItem containing the necessary information to begin moving the file</param>
        private void SendWorkItemToFileMover(HandBrakeWorkItem workItem)
        {
            lock (fileMover)
            {
                // Just in case
                fileMover.AddWorkItem(workItem);
            }
        }

        /// <summary>
        /// Stops the worker thread and if indicated stops the FileMover
        /// </summary>
        /// <param name="shouldStopFileMover">Should we stop the fileMover worker?</param>
        public void StopWorkerThread(bool shouldStopFileMover)
        {
            lock (threadLock)
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            if (shouldStopFileMover)
            {
                fileMover.StopWorkerThread();
                fileMover.Dispose();
            }
        }

        public void Dispose()
        {
            StopWorkerThread(true); // Stop the file mover as well

            // Log the other items left in the queue?
        }
    }
}
