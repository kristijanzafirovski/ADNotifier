namespace GimNotifier.Models
{
    public class ComputerEntry
    {
        public string Name { get; set; } = "";
        public string OUName { get; set; } = "";
        public string OUDN { get; set; } = "";
        public bool Checked { get; set; } = true;

        public override string ToString() => Name;
    }
}
