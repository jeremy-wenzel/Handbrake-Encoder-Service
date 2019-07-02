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

        private string movieSerahDirectory;
        private string movieDestinationDirectory;
        private string tvShowSearchDirectory;
        private string tvShowDestinationDirectory;

        public HandBrakeEncoderService()
        {
            InitializeComponent();
            InitializeSettings();
            InitializeSearchers();
        }

        private void InitializeSettings()
        {
            HandBrakeEventLogger.InitLogger("HandBrakeEventLoggerSrc", "HandBrakeEventLoggerLog");
           
            try
            {
                movieSerahDirectory = ConfigurationManager.AppSettings["movieDirectory"];
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
            movieFileSearcher = new FileSearcher(movieSerahDirectory, movieDestinationDirectory, MediaType.Movie);
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
