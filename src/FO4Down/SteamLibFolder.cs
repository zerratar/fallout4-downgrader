namespace Fallout4Downgrader
{
    public class SteamLibFolder
    {
        public string Path { get; set; }
        public string Label { get; set; }
        public string ContentId { get; set; }
        public string TotalSize { get; set; }
        public string UpdateCleanBytesTally { get; set; }
        public string TimeLastUpdateCorruption { get; set; }
        public string[] Apps { get; set; }
    }
}
