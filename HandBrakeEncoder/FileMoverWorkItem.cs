using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandBrakeEncoder
{
    public class FileMoverWorkItem
    {
        public string EncodedFilePath { get; private set; }

        public string DestinationFilePath { get; private set; }

        public FileMoverWorkItem(string encodedFilePath, string destinationFilePath)
        {
            this.EncodedFilePath = encodedFilePath;
            this.DestinationFilePath = destinationFilePath;
        }
    }
}
