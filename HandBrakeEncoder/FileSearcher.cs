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
        private const int SEARCH_SLEEP_TIME_MS = 5000; // Wait 5 seconds before scanning again.
        private volatile object threadLock = new object();
        private Thread thread = null;

        private volatile string searchDirectory = null;
        private volatile string destinationDirectory = null;
        private volatile MediaType expectedMediaType;

        public FileSearcher(string searchDirectory, string destinationDirectory, MediaType expectedMediaType)
        {
            this.searchDirectory = searchDirectory;
            this.destinationDirectory = destinationDirectory;
            this.expectedMediaType = expectedMediaType;
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

                if (files == null || files.Length == 0)
                {
                    // Couldn't find any files. Sleep and try again in some time
                    Thread.Sleep(SEARCH_SLEEP_TIME_MS);
                    continue;
                }

                // Found some items. Begin adding them to the queue
                foreach (string filePath in files)
                {
                    HandBrakeEncoderProcessor.AddWorkItem(
                        new HandBrakeWorkItem(
                            filePath, 
                            GetDestinationDirectoryFromFile(filePath), 
                            expectedMediaType));
                }
            }
        }

        private string GetDestinationDirectoryFromFile(string filePath)
        {
            // TODO: This should be based on the media type as well.
            return destinationDirectory;
        }

        /// <summary>
        /// Stops the working thread that is searching for new files.
        /// Also stops the HandBrakeEncoderProcessor that is encoding the files
        /// </summary>
        public void StopWorkerThread()
        {
            lock (threadLock)
            {
                if (thread != null && thread.IsAlive)
                {
                    thread.Abort();
                }
            }
        }
    }
}
