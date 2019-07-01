using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;

namespace HandBrakeEncoder
{
    public partial class HandBrakeEncoderService : ServiceBase
    {
        private FileSearcher fileSearcher;
        private static readonly HandBrakeEventLogger logger = HandBrakeEventLogger.GetInstance();

        public HandBrakeEncoderService()
        {
            InitializeComponent();
            HandBrakeEventLogger.InitLogger("HandBrakeEventLoggerSrc", "HandBrakeEventLoggerLog");
            try
            {
                string searchDirectory = ConfigurationManager.AppSettings["searchDirectory"];
                fileSearcher = new FileSearcher(searchDirectory);
            }
            catch (ConfigurationErrorsException e)
            {
                logger.WriteEntry(e.Message);
                throw;
            }
        }

        protected override void OnStart(string[] args)
        {
            fileSearcher.StartWorkerThread();
        }

        protected override void OnStop()
        {
            fileSearcher.StopWorkerThread(true);
        }
    }
}
