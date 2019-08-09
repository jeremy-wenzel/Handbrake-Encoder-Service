using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandBrakeEncoder
{
    public abstract class LogBase
    {
        public abstract void Log(string message);
    }

    public class FileLogger : LogBase
    {
        public string filePath = @"C:\HandbrakeEncoderLogger.log";

        public static FileLogger logger = new FileLogger();

        private static readonly object _lock = new object();
        public override void Log(string message)
        {
            lock (_lock)
            {
                using (StreamWriter streamWriter = new StreamWriter(filePath, true))
                {
                    streamWriter.WriteLine(message);
                    streamWriter.Close();
                }
            }
        }

        public static FileLogger GetInstance()
        {
            return logger;
        }
    }
}
