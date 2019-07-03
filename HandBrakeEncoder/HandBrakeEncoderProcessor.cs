using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        private volatile HandBrakeArguements arguments = new HandBrakeArguements();

        private HandBrakeEncoderProcessor()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="arguments"></param>
        public static void SetCommandLineArguments(HandBrakeArguements arguments)
        {
            if (arguments == null)
            {
                throw new ArgumentNullException("Arguments should not be null");
            }

            processor.arguments = arguments;
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
            string encodedFilePath = GetEncodedFilePath(workItem.OriginalFilePath);
            Process process = SetupProcessWithStartInfo(workItem.OriginalFilePath, encodedFilePath);
            process.Start();
            process.WaitForExit();

            // Send the workitem to the filemover to actually move the file
            string destinationPath = GetDestinationPathFromEncodeFile(encodedFilePath, workItem.DestinationDirectory);
            SendWorkItemToFileMover(encodedFilePath, destinationPath);
        }

        private string GetEncodedFilePath(string originalFilePath)
        {
            // TODO: Might think about moving to HandbrakeWorkItem?
            string directory = Path.GetDirectoryName(originalFilePath);
            string originalFileName = Path.GetFileNameWithoutExtension(originalFilePath);
            return Path.Combine(directory, $"{originalFileName}.mp4");
        }

        private Process SetupProcessWithStartInfo(string originalFilePath, string encodedFilePath)
        {
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = HandBrakeArguements.COMMAND;
            startInfo.Arguments = arguments.GenerateArguments(originalFilePath, encodedFilePath);
            logger.WriteEntry($"Process command line args = {startInfo.Arguments}");
            return process;
        }

        private string GetDestinationPathFromEncodeFile(string encodeFilePath, string destinationDirectory)
        {
            string encodedFileName = Path.GetFileName(encodeFilePath);
            return Path.Combine(destinationDirectory, encodedFileName);
        }

        /// <summary>
        /// Sends the work item to the file mover
        /// </summary>
        /// <param name="workItem">The workItem containing the necessary information to begin moving the file</param>
        private void SendWorkItemToFileMover(string encodedFilePath, string destinationFilePath)
        {
            lock (fileMover)
            {
                // Just in case
                fileMover.AddWorkItem(new FileMoverWorkItem(encodedFilePath, destinationFilePath));
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
