using System.IO;
using System.Threading;

namespace HandBrakeEncoder
{
    /// <summary>
    /// Class that searches for the files and sends them to the
    /// HandBrakeEncoderProcessor to be processed.
    /// </summary>
    public class FileSearcher
    {
        private static readonly HandBrakeEventLogger logger = HandBrakeEventLogger.GetInstance();
        private volatile object threadLock = new object();
        private Thread thread = null;

        private volatile HandBrakeEncoderProcessor encoderProcessor = new HandBrakeEncoderProcessor();

        private string searchDirectory = null;

        public FileSearcher(string searchDirectory)
        {
            this.searchDirectory = searchDirectory;
        }

        /// <summary>
        /// Starts the working thread that begins search for files.
        /// Also begins starting the HandBrakeEncoderProcessor to process encoded files
        /// </summary>
        public void StartWorkerThread()
        {
            lock (threadLock)
            {
                if (thread == null || !thread.IsAlive)
                {
                    thread = new Thread(new ThreadStart(SearchForFiles));
                    thread.Start();
                    encoderProcessor.StartWorkerThread();
                }
            }
        }

        /// <summary>
        /// Stops the working thread that is searching for new files.
        /// Also stops the HandBrakeEncoderProcessor that is encoding the files
        /// </summary>
        /// <param name="shouldStopEncoderThread"></param>
        public void StopWorkerThread(bool shouldStopEncoderThread)
        {
            lock (threadLock)
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
                if (shouldStopEncoderThread)
                {
                    encoderProcessor.StopWorkerThread(true);
                }
            }
        }

        /// <summary>
        /// Thread method that looks for the files and sends
        /// them to the HandBrakeEncoderProcessor
        /// </summary>
        private void SearchForFiles()
        {
            if (!Directory.Exists(searchDirectory))
            {
                throw new DirectoryNotFoundException($"Could not find directory {searchDirectory}");
            }

            while (true)
            {
                string[] files = Directory.GetFiles(searchDirectory, ".mkv", SearchOption.AllDirectories);

                foreach (string filePath in files)
                {
                    encoderProcessor.AddWorkItem(new HandBrakeWorkItem(filePath));
                }
            }
        }
    }
}
