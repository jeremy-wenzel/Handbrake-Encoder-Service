using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandBrakeEncoder
{
    public class HandBrakeArguements
    {
        public const string COMMAND = @"C:\HandBrakeCLI.exe";

        public string GenerateArguments(string originalFilePath, string encodedFilePath)
        {
            /**
             * Usage: HandBrakeCLI [options] -i <source> -o <destination>
             * 
             * See more at https://handbrake.fr/docs/en/latest/cli/command-line-reference.html
             */

            
            return $"{GenerateOptionsFlags()} -i {originalFilePath} -o {encodedFilePath}";
        }

        private string GenerateOptionsFlags()
        {
            return String.Empty;
        }
    }
}
