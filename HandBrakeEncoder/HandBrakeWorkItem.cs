using System.IO;
using System.Linq;

namespace HandBrakeEncoder
{
    /// <summary>
    /// The workitem used by the various processor that contains the information
    /// necessary to begin encoding and moving of the file
    /// </summary>
    public class HandBrakeWorkItem
    {
        /// <summary>
        /// The path of the original file
        /// </summary>
        public string OriginalFilePath { get; private set; }

        public string DestinationDirectory { get; private set; }

        public MediaType MediaType { get; private set; }

        public HandBrakeWorkItem(string originalFilePath, string destinationDirectory, MediaType mediaType)
        {
            this.OriginalFilePath = originalFilePath;
            this.DestinationDirectory = destinationDirectory;
            this.MediaType = mediaType;
        }
    }

    public enum MediaType
    {
        Movie,
        TvShow
    }
}
