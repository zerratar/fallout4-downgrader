namespace FO4Down
{
    public class ChunkDownloadProgress
    {
        private ApplicationContext stepContext;
        private DateTime startTime;

        public ChunkDownloadProgress(ApplicationContext stepContext)
        {
            this.stepContext = stepContext;
            this.startTime = DateTime.UtcNow;
        }

        public ulong Offset { get; internal set; }
        public uint ChunkSize { get; internal set; }
        public double Fraction { get; internal set; }
        public long BytesDownloaded { get; set; }
        public long ActualLength { get; set; }
        public string FileName { get; internal set; }
        public double KiloBytesPerSecond { get; set; }
        public DateTime CompletedTime { get; private set; }
        public bool Completed { get; private set; }
        internal void Progress(long totalRead, long totalLength, double v)
        {
            BytesDownloaded = totalRead;
            ActualLength = totalLength;
            Fraction = v;

            CalculateDownloadSpeed();
        }

        internal void SetCompleted()
        {
            CompletedTime = DateTime.UtcNow;
            Completed = true;
        }

        private void CalculateDownloadSpeed()
        {
            var currentTime = DateTime.UtcNow;
            var timeSpan = currentTime - startTime;
            if (timeSpan.TotalSeconds > 0) // Ensure we do not divide by zero
            {
                KiloBytesPerSecond = (BytesDownloaded / 1024.0) / timeSpan.TotalSeconds;
            }
        }
    }
}
