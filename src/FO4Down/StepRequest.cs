namespace FO4Down
{
    public abstract class StepRequest
    {
        public string Name { get; set; }
        public string[] Arguments { get; internal set; }

        protected object Value;
        protected TaskCompletionSource<object> src;

        public void SetResult(object value)
        {
            this.Value = value;
            src.SetResult(value);
        }

        protected StepRequest()
        {
            this.src = new TaskCompletionSource<object>();
        }
    }
}
