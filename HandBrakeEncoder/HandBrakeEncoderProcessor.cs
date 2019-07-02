using System;
using System.Collections.Generic;
using System.Threading;

namespace HandBrakeEncoder
{
    /// <summary>
    /// Takes items from a work queue and begins encoding them with Hand Brake.
    /// This class should be a singleton because we only want to have one place were we are
    /// calling Handbrake to encode
    /// </summary>
    public class HandBrakeEncoderProcessor : IDisposable
    {
        private const int MAX_CYCLES_FOR_PROCESSING_THREAD = 100;

        private static readonly HandBrakeEventLogger logger = HandBrakeEventLogger.GetInstance();
        private volatile Queue<HandBrakeWorkItem> workItems = new Queue<HandBrakeWorkItem>();
        private volatile object threadLock = new object();

        private Thread thread = null;

        private volatile FileMover fileMover = new FileMover();
        public FileMover FileMover => fileMover;

        private static readonly HandBrakeEncoderProcessor processor = new HandBrakeEncoderProcessor();

        private HandBrakeEncoderProcessor()
        {

        }

        /// <summary>
        /// Adds the work item to the processor queue
        /// </summary>
        /// <param name="workItem">The work item to be processed</param>
        public static void AddWorkItem(HandBrakeWorkItem workItem)
        {
            processor.AddWorkItemInternal(workItem);
        }

        /// <summary>
        /// Adds the work item to the processor queue and kicks off a worker thread
        /// to begin the processing if a worker thread doesn't exist
        /// </summary>
        /// <param name="workItem"></param>
        private void AddWorkItemInternal(HandBrakeWorkItem workItem)
        {
            lock (workItems)
            {
                workItems.Enqueue(workItem);
            }
            StartWorkerThread();
        }

        /// <summary>
        /// Starts a working thread to begin looking for items to encode and start encoding them.
        /// It also kicks off another thread for the file move to being looking for files to move.
        /// </summary>
        private void StartWorkerThread()
        {
            lock (threadLock)
            {
                if (thread == null || !thread.IsAlive)
                {
                    // Only start if the thread isn't alive or if the thread doesn't exist
                    thread = new Thread(new ThreadStart(ProcessWorkQueueAndEncode));
                    thread.Start();
                }
            }
        }

        /// <summary>
        /// Goes through the work items and begins encoding.
        /// </summary>
        private void ProcessWorkQueueAndEncode()
        {
            int currentCycle = 0;
            while (currentCycle < MAX_CYCLES_FOR_PROCESSING_THREAD)
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
                    currentCycle++;
                    logger.WriteEntry("Didn't find work item. Sleeping");
                    continue;
                }

                // Found a work item. Beginning Encoding and reset cycle count
                currentCycle = 0;
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
        /// Stops the processor from encoding
        /// </summary>
        public static void StopProcessor()
        {
            processor.Dispose();
        }

        public void Dispose()
        {
            StopWorkerThread(); // Stop the file mover as well

            // Log the other items left in the queue?
        }

        /// <summary>
        /// Stops the worker thread and if indicated stops the FileMover
        /// </summary>
        private void StopWorkerThread()
        {
            lock (threadLock)
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            fileMover.Dispose();
        }
    }
}
