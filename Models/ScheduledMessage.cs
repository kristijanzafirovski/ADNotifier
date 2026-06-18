namespace GimNotifier.Models
{
    public class ScheduledMessage
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime SendAt { get; set; }
        public string Message { get; set; } = "";
        public List<string> Computers { get; set; } = new();
        public string OULabel { get; set; } = "";
        public bool Sent { get; set; } = false;
        public bool Cancelled { get; set; } = false;
    }
}
