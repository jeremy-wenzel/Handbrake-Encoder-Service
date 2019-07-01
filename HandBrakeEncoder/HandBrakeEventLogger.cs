using System.Diagnostics;
using System.Text;

namespace HandBrakeEncoder
{
    /// <summary>
    /// An extension of EventLogger to log events during the Handbrake process. This is meant to be a Singleton class
    /// so that multiple threads and files can add logs
    /// </summary>
    public class HandBrakeEventLogger : EventLog
    {
        private static HandBrakeEventLogger logger;
        private StringBuilder buffer = new StringBuilder();

        private HandBrakeEventLogger(string source, string log)
        {
            if (!EventLog.SourceExists(source))
            {
                EventLog.CreateEventSource(source, log);
            }
            Source = source;
            Log = log;
        }

        /// <summary>
        /// Inits the logger to be used for the service. Needs to be done before the service truly stats
        /// </summary>
        /// <param name="source"></param>
        /// <param name="log"></param>
        public static void InitLogger(string source, string log)
        {
            if (logger != null)
            {
                logger = new HandBrakeEventLogger(source, log);
            }
        }

        public static HandBrakeEventLogger GetInstance()
        {
            return logger;
        }

        /// <summary>
        /// Writes a string entry to the EventLog. Appends the log with the stack trace.
        /// </summary>
        /// <param name="message"></param>
        public new void WriteEntry(string message)
        {
            StackTrace stackTrace = new StackTrace(1, false);
            lock (buffer) 
            {
                buffer.Append(stackTrace.GetFrame(0).GetMethod().DeclaringType.Name);
                buffer.Append(message);
                base.WriteEntry(buffer.ToString());
                buffer.Clear();
            }
        }
    }
}
