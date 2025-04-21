public class AccessLog
{
    public DateTime TimeStamp { get; set; }
    public string IDNumber { get; set; }
    public string Name { get; set; }
    public string Department { get; set; }
    public string UserType { get; set; }
    public string EventType { get; set; } // ENTRY or EXIT
}
