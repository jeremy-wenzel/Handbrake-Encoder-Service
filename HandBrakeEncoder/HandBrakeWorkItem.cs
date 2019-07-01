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

        /// <summary>
        /// The path of the encoded file
        /// </summary>
        public string EncodedFilePath { get; set; }

        public MediaType MediaType { get; set; }

        public HandBrakeWorkItem(string originalFilePath, MediaType mediaType)
        {
            this.OriginalFilePath = originalFilePath;
            this.MediaType = mediaType;
        }
    }

    public enum MediaType
    {
        Movie,
        TvShow
    }
}
