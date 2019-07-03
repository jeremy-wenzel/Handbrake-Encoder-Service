using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace HandBrakeEncoder
{
    public partial class HandBrakeEncoderService : ServiceBase
    {
        private FileSearcher movieFileSearcher;
        private FileSearcher tvShowFileSearcher;

        private static readonly HandBrakeEventLogger logger = HandBrakeEventLogger.GetInstance();

        private string movieSearchDirectory;
        private string movieDestinationDirectory;
        private string tvShowSearchDirectory;
        private string tvShowDestinationDirectory;

        public HandBrakeEncoderService()
        {
            InitializeComponent();
            InitializeProcessor();
            InitializeSettings();
            InitializeSearchers();
        }

        private void InitializeProcessor()
        {
            HandBrakeEncoderProcessor.SetCommandLineArguments(new HandBrakeArguements());
        }

        private void InitializeSettings()
        {
            HandBrakeEventLogger.InitLogger("HandBrakeEventLoggerSrc", "HandBrakeEventLoggerLog");
           
            try
            {
                movieSearchDirectory = ConfigurationManager.AppSettings["movieDirectory"];
                movieDestinationDirectory = ConfigurationManager.AppSettings["movieOutputDirectory"];
                tvShowSearchDirectory = ConfigurationManager.AppSettings["tvShowDirectory"];
                tvShowDestinationDirectory = ConfigurationManager.AppSettings["tvhShowOutputDirectory"];
            }
            catch (ConfigurationErrorsException e)
            {
                logger.WriteEntry(e.Message);
                throw;
            }
        }

        private void InitializeSearchers()
        {
            movieFileSearcher = new FileSearcher(movieSearchDirectory, movieDestinationDirectory, MediaType.Movie);
            tvShowFileSearcher = new FileSearcher(tvShowSearchDirectory, tvShowDestinationDirectory, MediaType.TvShow);
        }

        protected override void OnStart(string[] args)
        {
            StartFileSearchers();
        }

        private void StartFileSearchers()
        {
            movieFileSearcher.StartWorkerThread();
            tvShowFileSearcher.StartWorkerThread();
        }

        protected override void OnStop()
        {
            StopFileSearchers();
            HandBrakeEncoderProcessor.StopProcessor();
        }

        private void StopFileSearchers()
        {
            movieFileSearcher.StopWorkerThread();
            tvShowFileSearcher.StopWorkerThread();
        }
    }
}
