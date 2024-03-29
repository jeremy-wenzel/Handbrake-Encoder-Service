﻿using System.ServiceProcess;

namespace HandBrakeEncoder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new HandBrakeEncoderService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
