namespace GimNotifier.Models
{
    public class MessageTemplate
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public string Body { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
